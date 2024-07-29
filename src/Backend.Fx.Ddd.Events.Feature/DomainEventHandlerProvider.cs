using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Ddd.Events.Feature;

public class DomainEventHandlerProvider
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventHandlerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>()
        where TDomainEvent : IDomainEvent
    {
        Type eventType = typeof(TDomainEvent);
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        return _serviceProvider.GetServices(handlerType).Cast<IDomainEventHandler<TDomainEvent>>();
    }
}