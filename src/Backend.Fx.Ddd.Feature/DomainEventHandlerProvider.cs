using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Ddd.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Ddd.Feature;

public class DomainEventHandlerProvider
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventHandlerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>()
        where TDomainEvent : class
    {
        Type eventType = typeof(TDomainEvent);
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        return _serviceProvider.GetServices(handlerType).Cast<IDomainEventHandler<TDomainEvent>>();
    }
    
    public IEnumerable<object> GetAllEventHandlers(Type domainEventType)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEventType);
        return _serviceProvider.GetServices(handlerType)!;
    }
}