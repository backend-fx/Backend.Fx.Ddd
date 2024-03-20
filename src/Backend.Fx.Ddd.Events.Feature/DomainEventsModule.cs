using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Ddd.Events.Feature;

internal class DomainEventsModule : IModule
{
    private readonly ILogger _logger = Log.Create<DomainEventsModule>();
    private readonly Assembly[] _assemblies;

    public DomainEventsModule(params Assembly[] assemblies)
    {
        _assemblies = assemblies;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        compositionRoot.Register(
            ServiceDescriptor.Scoped(sp => new DomainEventAggregator(new DomainEventHandlerProvider(sp))));

        compositionRoot.Register(
            ServiceDescriptor.Scoped<IDomainEventAggregator>(sp => sp.GetRequiredService<DomainEventAggregator>()));

        compositionRoot.Register(
            ServiceDescriptor.Scoped<IDomainEventPublisher>(sp => sp.GetRequiredService<DomainEventAggregator>()));

        compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, RaiseDomainEventsOperationDecorator>());

        RegisterDomainEventHandlers(compositionRoot);
    }

    private void RegisterDomainEventHandlers(ICompositionRoot compositionRoot)
    {
        foreach (Type domainEventType in _assemblies.GetImplementingTypes(typeof(IDomainEvent)))
        {
            Type handlerTypeForThisDomainEventType = typeof(IDomainEventHandler<>).MakeGenericType(domainEventType);

            var serviceDescriptors = _assemblies
                .GetImplementingTypes(handlerTypeForThisDomainEventType)
                .Select(t => new ServiceDescriptor(handlerTypeForThisDomainEventType, t, ServiceLifetime.Scoped))
                .ToArray();

            if (serviceDescriptors.Any())
            {
                compositionRoot.RegisterCollection(serviceDescriptors);
            }
            else
            {
                _logger.LogWarning("No handlers for {DomainEventType} found", domainEventType);
            }
        }
    }


    [UsedImplicitly]
    public class RaiseDomainEventsOperationDecorator : IOperation
    {
        private readonly IDomainEventAggregator _domainEventAggregator;
        private readonly IOperation _operation;

        public RaiseDomainEventsOperationDecorator(
            IDomainEventAggregator domainEventAggregator,
            IOperation operation)
        {
            _domainEventAggregator = domainEventAggregator;
            _operation = operation;
        }

        public Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
        {
            return _operation.BeginAsync(serviceScope, cancellationToken);
        }

        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            await _domainEventAggregator.RaiseEventsAsync(cancellationToken).ConfigureAwait(false);
            await _operation.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task CancelAsync(CancellationToken cancellationToken = default)
        {
            return _operation.CancelAsync(cancellationToken);
        }
    }

    private class DomainEventHandlerProvider : IDomainEventHandlerProvider
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
}