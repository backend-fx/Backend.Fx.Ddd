using System.Reflection;
using Backend.Fx.Ddd.Feature;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using Xunit;

namespace Backend.Fx.Ddd.Tests;

public class TheDomainServiceFeature : IAsyncLifetime
{
    private readonly IBackendFxApplication _app;

    public TheDomainServiceFeature()
    {
        _app = new TestApplication(GetType().Assembly);
        _app.AddFeature(new DomainServicesFeature());
    }

    public async ValueTask InitializeAsync()
    {
        await _app.BootAsync();
    }

    [Fact]
    public void RegistersAndResolvesDomainService()
    {
        using var scope = _app.CompositionRoot.BeginScope();
        _ = scope.ServiceProvider.GetRequiredService<IMyService>();
    }

    [Fact]
    public void DomainServicesAreScoped()
    {
        Assert.Throws<ActivationException>(
            () => _app.CompositionRoot.ServiceProvider.GetRequiredService<IMyService>());
    }

    public ValueTask DisposeAsync()
    {
        _app.Dispose();
        return ValueTask.CompletedTask;
    }


    private class TestApplication(params Assembly[] assemblies)
        : BackendFxApplication(
            new SimpleInjectorCompositionRoot(),
            new DebugExceptionLogger(),
            assemblies);

    [UsedImplicitly]
    public interface IMyService : IDomainService;

    [UsedImplicitly]
    public class MyService : IMyService;
}