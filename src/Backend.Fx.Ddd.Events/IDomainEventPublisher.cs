namespace Backend.Fx.Ddd.Events;

/// <summary>
/// The API to publish domain events.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publish a domain event that is handled by all handlers asynchronously in the same scope/transaction.
    /// Possible exceptions are not caught, so that your action might fail due to a failing event handler.
    /// </summary>
    /// <typeparam name="TDomainEvent"></typeparam>
    /// <param name="domainEvent"></param>
    void PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent;
}