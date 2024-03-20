using System.Collections.Generic;

namespace Backend.Fx.Ddd.Events;

public interface IDomainEventHandlerProvider
{
    /// <summary>
    /// get all domain event handlers that want to handle a specific domain event
    /// </summary>
    /// <typeparam name="TDomainEvent"></typeparam>
    /// <returns></returns>
    IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>() where TDomainEvent : IDomainEvent;
}