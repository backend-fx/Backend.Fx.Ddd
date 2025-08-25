using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Ddd.Feature;

internal class DomainServicesModule : IModule
{
    private readonly ILogger _logger = Log.Create<DomainServicesModule>();
    private readonly IEnumerable<Assembly> _assemblies;
    
    public DomainServicesModule(IEnumerable<Assembly> assemblies)
    {
        _assemblies = assemblies;
        
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        RegisterDomainServices(compositionRoot);
    }

    private void RegisterDomainServices(ICompositionRoot container)
    {
        var assembliesForLogging = string.Join(",", _assemblies.Select(ass => ass.GetName().Name));
        _logger.LogDebug("Registering domain and application services from {Assemblies}", assembliesForLogging);

        var serviceDescriptors = _assemblies
                                 .GetImplementingTypes(typeof(IDomainService))
                                 .SelectMany(type =>
                                     type.GetTypeInfo()
                                         .ImplementedInterfaces
                                         .Where(i => typeof(IDomainService) != i &&
                                                     _assemblies.Contains(i.GetTypeInfo().Assembly))
                                         .Select(service =>
                                             new ServiceDescriptor(service, type, ServiceLifetime.Scoped)));


        foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
        {
            _logger.LogDebug("Registering scoped service {ServiceType} with implementation {ImplementationType}",
                serviceDescriptor.ServiceType.Name,
                serviceDescriptor.ImplementationType?.Name ?? "dynamic");

            container.Register(serviceDescriptor);
        }
    }
}