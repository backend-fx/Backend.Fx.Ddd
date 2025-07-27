using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Ddd.Events;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Ddd.Feature;

internal class DomainEventsModule : IModule
{
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
        var handlerTypes = _assemblies
            .Where(ass => !ass.IsDynamic)
            .SelectMany(ass => ass.GetTypes())
            .Where(t => !t.IsAbstract && t.IsClass)
            .Where(t => t.IsImplementationOfOpenGenericInterface(typeof(IDomainEventHandler<>)));

        var domainEventTypeToHandlerTypesMap = new Dictionary<Type, List<Type>>();

        foreach (var handlerType in handlerTypes)
        {
            // a handler could handle various domain events, so we have to loop over the interface
            var implementedInterfaces = handlerType
                .GetInterfaces()
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                .ToArray();

            foreach (var implementedInterface in implementedInterfaces)
            {
                var domainEventType = implementedInterface.GenericTypeArguments.Single();
                if (domainEventTypeToHandlerTypesMap.ContainsKey(domainEventType))
                {
                    domainEventTypeToHandlerTypesMap[domainEventType].Add(handlerType);
                }
                else
                {
                    domainEventTypeToHandlerTypesMap[domainEventType] = new List<Type>([handlerType]);
                }
            }
        }

        foreach (var registration in domainEventTypeToHandlerTypesMap)
        {
            var domainEventType = registration.Key;
            var handlerService = typeof(IDomainEventHandler<>).MakeGenericType(domainEventType);
            var serviceDescriptors = registration.Value.Select(
                handlerType => new ServiceDescriptor(handlerService, handlerType, ServiceLifetime.Scoped)).ToArray();
            compositionRoot.RegisterCollection(serviceDescriptors);
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
}