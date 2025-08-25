using System.Collections.Generic;
using System.Reflection;
using Backend.Fx.Execution;
using Backend.Fx.Execution.Features;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd.Feature;

/// <summary>
/// The feature "Domain Services" makes sure that all implementations of <see cref="IDomainService"/>
/// are injected as scoped instances.
/// </summary>
[PublicAPI]
public class DomainServicesFeature : IFeature
{
    public void Enable(IBackendFxApplication application)
    {
        application.CompositionRoot.RegisterModules(new DomainServicesModule(application.Assemblies));
    }

    public IEnumerable<Assembly> Assemblies { get; } = [];
}