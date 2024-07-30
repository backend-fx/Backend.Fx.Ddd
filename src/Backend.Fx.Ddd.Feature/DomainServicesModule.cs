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
    private readonly Assembly[] _assemblies;
    private readonly string _assembliesForLogging;

    public DomainServicesModule(params Assembly[] assemblies)
    {
        _assemblies = assemblies;
        _assembliesForLogging = string.Join(",", _assemblies.Select(ass => ass.GetName().Name));
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        RegisterDomainServices(compositionRoot);
    }

    private void RegisterDomainServices(ICompositionRoot container)
    {
        _logger.LogDebug("Registering domain and application services from {Assemblies}", _assembliesForLogging);

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