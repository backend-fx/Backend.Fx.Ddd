using Backend.Fx.Execution;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd.Feature;

/// <summary>
/// The feature "Id Generation" makes sure that all implementations of <see cref="IIdGenerator{TId}"/> are registered
/// as singleton instances. 
/// </summary>
[PublicAPI]
public class IdGenerationFeature : Backend.Fx.Execution.Features.Feature
{
    public override void Enable(IBackendFxApplication application)
    {
        application.CompositionRoot.RegisterModules(new IdGenerationModule(application.Assemblies));
    }
}