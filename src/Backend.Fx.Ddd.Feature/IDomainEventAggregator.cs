using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Ddd.Events;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Ddd.Feature;

/// <summary>
/// Channel events from multiple objects into a single object to simplify registration for clients.
/// https://martinfowler.com/eaaDev/EventAggregator.html
/// </summary>
public interface IDomainEventAggregator
{
    Task RaiseEventsAsync(CancellationToken cancellationToken = default);
}

public class DomainEventAggregator : IDomainEventAggregator, IDomainEventPublisher
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethods = new();
    private readonly ILogger _logger = Log.Create<DomainEventAggregator>();
    private readonly DomainEventHandlerProvider _domainEventHandlerProvider;
    private readonly ConcurrentQueue<HandleAction> _handleActions = new();
    
    public DomainEventAggregator(DomainEventHandlerProvider domainEventHandlerProvider)
    {
        _domainEventHandlerProvider = domainEventHandlerProvider;
    }
    
    public void PublishDomainEvent(IDomainEvent domainEvent)
    {
        var domainEventType = domainEvent.GetType();

        foreach (var injectedHandler in _domainEventHandlerProvider.GetAllEventHandlers(domainEventType))
        {
            var handleMethod = HandleMethods.GetOrAdd(
                injectedHandler.GetType(),
                type => type.GetMethod("HandleAsync", BindingFlags.Instance | BindingFlags.Public));
                
            var handleAction = new HandleAction(
                domainEventType,
                injectedHandler.GetType(),
                ct => (Task)handleMethod.Invoke(injectedHandler, new object[] { domainEvent, ct }));

            _handleActions.Enqueue(handleAction);
            _logger.LogDebug(
                "Invocation of {HandlerTypeName} for domain event {DomainEvent} registered. It will be executed on completion of operation",
                injectedHandler.GetType().Name,
                domainEvent);
        }
    }

    public void PublishDomainEvents(IHaveDomainEvents entity)
    {
        PublishDomainEventsFromOutBox(entity.DomainEvents);
    }
    
    public void PublishDomainEventsFromOutBox(DomainEventOutBox outBox)
    {
        foreach (var domainEvent in outBox)
        {
            PublishDomainEvent(domainEvent);
        }
    }


    public async Task RaiseEventsAsync(CancellationToken cancellationToken = default)
    {
        while (_handleActions.TryDequeue(out var handleAction))
        {
            await handleAction.InvokeAsync(cancellationToken);
        }
    }

    private class HandleAction
    {
        private readonly Type _domainEventType;
        private readonly Type _handlerType;
        private readonly Func<CancellationToken, Task> _asyncAction;

        public HandleAction(Type domainEventType, Type handlerType, Func<CancellationToken, Task> asyncAction)
        {
            _domainEventType = domainEventType;
            _handlerType = handlerType;
            _asyncAction = asyncAction;
        }


        public async Task InvokeAsync(CancellationToken cancellationToken)
        {
            var logger = Log.Create(_handlerType);

            try
            {
                await _asyncAction.Invoke(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Handling of {DomainEvent} by {HandlerTypeName} failed",
                    _domainEventType.Name,
                    _handlerType.Name);
                throw;
            }
        }
    }
}