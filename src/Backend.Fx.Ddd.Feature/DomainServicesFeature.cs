using Backend.Fx.Execution;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd.Feature
{
    /// <summary>
    /// The feature "Domain Services" makes sure that all implementations of <see cref="IDomainService"/>
    /// are injected as scoped instances.
    /// </summary>
    [PublicAPI]
    public class DomainServicesFeature : Execution.Features.Feature
    {
        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new DomainServicesModule(application.Assemblies));
        }
    }
}