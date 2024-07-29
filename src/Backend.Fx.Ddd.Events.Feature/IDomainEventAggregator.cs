using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Ddd.Events.Feature;

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
    private readonly ILogger _logger = Log.Create<DomainEventAggregator>();
    private readonly DomainEventHandlerProvider _domainEventHandlerProvider;
    private readonly ConcurrentQueue<HandleAction> _handleActions = new();

    public DomainEventAggregator(DomainEventHandlerProvider domainEventHandlerProvider)
    {
        _domainEventHandlerProvider = domainEventHandlerProvider;
    }

    public void PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
    {
        foreach (var injectedHandler in _domainEventHandlerProvider.GetAllEventHandlers<TDomainEvent>())
        {
            var handleAction = new HandleAction(
                typeof(TDomainEvent),
                injectedHandler.GetType(),
                ct => injectedHandler.HandleAsync(domainEvent, ct));

            _handleActions.Enqueue(handleAction);
            _logger.LogDebug(
                "Invocation of {HandlerTypeName} for domain event {DomainEvent} registered. It will be executed on completion of operation",
                injectedHandler.GetType().Name,
                domainEvent);
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