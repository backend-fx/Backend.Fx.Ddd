using Backend.Fx.Execution;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd.Feature;

/// <summary>
/// The feature "Open Host Services" makes sure that all implementations of <see cref="IOpenHostService"/>
/// are injected as singleton instances.
/// </summary>
[PublicAPI]
public class OpenHostServicesFeature : Backend.Fx.Execution.Features.Feature
{
    public override void Enable(IBackendFxApplication application)
    {
        application.CompositionRoot.RegisterModules(new OpenHostServicesModule(application.Assemblies));
    }
}