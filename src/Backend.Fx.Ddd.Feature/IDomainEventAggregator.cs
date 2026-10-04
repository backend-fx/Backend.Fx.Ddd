using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
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

    public void PublishDomainEvent(object domainEvent) 
    {
        var domainEventType = domainEvent.GetType();
        var handleMethod = HandleMethods.GetOrAdd(
            domainEventType,
            t => typeof(IDomainEventHandler<>)
                     .MakeGenericType(t)
                     .GetMethod(nameof(IDomainEventHandler<object>.HandleAsync))
                 ?? throw new InvalidOperationException(
                     $"IDomainEventHandler<{t.Name}>.HandleAsync could not be found"));

        foreach (var injectedHandler in _domainEventHandlerProvider.GetAllEventHandlers(domainEventType))
        {
            var handler = injectedHandler;
            var handleAction = new HandleAction(
                domainEventType,
                handler.GetType(),
                ct => InvokeHandleAsync(handleMethod, handler, domainEvent, ct));

            _handleActions.Enqueue(handleAction);
            _logger.LogDebug(
                "Invocation of {HandlerTypeName} for domain event {DomainEvent} registered. It will be executed on completion of operation",
                handler.GetType().Name,
                domainEvent);
        }
    }

    /// <summary>
    ///     Invokes the handler through the (closed) <see cref="IDomainEventHandler{TDomainEvent}" /> interface method,
    ///     which also works for explicit interface implementations, and rethrows the original exception instead of the
    ///     wrapping <see cref="TargetInvocationException" />.
    /// </summary>
    private static Task InvokeHandleAsync(
        MethodInfo handleMethod,
        object handler,
        object domainEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            return (Task)handleMethod.Invoke(handler, new[] { domainEvent, cancellationToken })!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw; // never reached
        }
    }

    public void PublishDomainEvents(IHaveDomainEvents entity)
    {
        PublishDomainEventsFromOutBox(entity.DomainEvents);
    }

    public void PublishDomainEventsFromOutBox(DomainEventOutBox outBox)
    {
        // draining prevents the same events from being published again on a subsequent call
        foreach (var domainEvent in outBox.Drain())
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

    private sealed class HandleAction
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