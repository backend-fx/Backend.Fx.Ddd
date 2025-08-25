using System.Reflection;
using System.Threading.Tasks;
using Backend.Fx.Ddd.Feature;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Fx.Ddd.Tests;

public class TheOpenHostServiceFeature : IAsyncLifetime
{
    private readonly IBackendFxApplication _app;

    public TheOpenHostServiceFeature()
    {
        _app = new TestApplication(GetType().Assembly);
        _app.AddFeature(new OpenHostServicesFeature());
    }

    public async Task InitializeAsync()
    {
        await _app.BootAsync();
    }

    [Fact]
    public void RegistersAndResolvesOpenHostService()
    {
        using var scope = _app.CompositionRoot.BeginScope();
        _ = scope.ServiceProvider.GetRequiredService<IMyService>();
    }

    [Fact]
    public void OpenHostServicesAreSingletons()
    {
        _ = _app.CompositionRoot.ServiceProvider.GetRequiredService<IMyService>();
    }

    public Task DisposeAsync()
    {
        _app.Dispose();
        return Task.CompletedTask;
    }


    private class TestApplication(params Assembly[] assemblies)
        : BackendFxApplication(
            new SimpleInjectorCompositionRoot(),
            new DebugExceptionLogger(),
            assemblies);

    [UsedImplicitly]
    public interface IMyService : IOpenHostService;

    [UsedImplicitly]
    public class MyService : IMyService;
}