using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Ddd.Feature;

public class IdGenerationModule : IModule
{
    private readonly ILogger _logger = Log.Create<DomainEventsModule>();
    private readonly Assembly[] _assemblies;

    public IdGenerationModule(params Assembly[] assemblies)
    {
        _assemblies = assemblies;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        RegisterIdGenerators(compositionRoot);
    }
    
    private void RegisterIdGenerators(ICompositionRoot compositionRoot)
    {
        var genericIdGeneratorType = typeof(IIdGenerator<>);

        var idGeneratorTypes = _assemblies
            .Where(ass => !ass.IsDynamic)
            .SelectMany(ass => ass.GetTypes())
            .Where(t => !t.IsAbstract && t.IsClass)
            .Where(t => t.IsImplementationOfOpenGenericInterface(genericIdGeneratorType))
            .ToArray();

        if (idGeneratorTypes.Length == 0)
        {
            _logger.LogWarning("No implementors of IIdGenerator<> found");
        }
        
        foreach (var idGeneratorType in idGeneratorTypes)
        {
            var implementedInterfaces = idGeneratorType
                .GetInterfaces()
                .Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == genericIdGeneratorType)
                .ToArray();

            foreach (var implementedInterface in implementedInterfaces)
            {
                var idType = implementedInterface.GetGenericArguments().Single();
                var serviceType = genericIdGeneratorType.MakeGenericType(idType);
                compositionRoot.Register(ServiceDescriptor.Singleton(serviceType, idGeneratorType));
                _logger.LogDebug(
                    "Registered singleton IIdGenerator<{IdType}>: {Type}",
                    idType.GetDetailedTypeName(),
                    idGeneratorType.GetDetailedTypeName());
            }
        }
    }
}