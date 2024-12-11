using System;
using System.Reflection;
using System.Threading.Tasks;
using Backend.Fx.Ddd.Feature;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Fx.Ddd.Tests;

public class TheIdGenerationFeature : IAsyncLifetime
{
    private readonly IBackendFxApplication _app;

    public TheIdGenerationFeature()
    {
        _app = new TestApplication(GetType().Assembly);
        _app.EnableFeature(new IdGenerationFeature());
    }

    public async Task InitializeAsync()
    {
        await _app.BootAsync();
    }

    [Fact]
    public void IdGeneratorsAreRegisteredAsSingletons()
    {
        var singleton = _app.CompositionRoot.ServiceProvider.GetRequiredService<IIdGenerator<ThatId>>();
        
        using var scope = _app.CompositionRoot.BeginScope();
        var scoped = scope.ServiceProvider.GetRequiredService<IIdGenerator<ThatId>>();
        
        Assert.StrictEqual(singleton, scoped);
    }
    
    [Fact]
    public void RegistersAndResolvesIdGenerators()
    {
        using var scope = _app.CompositionRoot.BeginScope();
        var thatIdGenerator = scope.ServiceProvider.GetRequiredService<IIdGenerator<ThatId>>();
        var thatId1 = thatIdGenerator.NextId();
        var thatId2 = thatIdGenerator.NextId();
        Assert.NotEqual(thatId1, thatId2);
        
        var thisIdGenerator = scope.ServiceProvider.GetRequiredService<IIdGenerator<ThisId>>();
        var thisId1 = thisIdGenerator.NextId();
        var thisId2 = thisIdGenerator.NextId();
        Assert.NotEqual(thisId1, thisId2);
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


    public class ThatId(Guid value) : Id<Guid>(value), IEquatable<ThatId>
    {
        public bool Equals(ThatId? other) => base.Equals(other);
    }

    public class ThisId(long value) : LongId(value), IEquatable<ThisId>
    {
        public bool Equals(ThisId? other) => base.Equals(other);
    }

    public class ThatIdGenerator : IIdGenerator<ThatId>
    {
        public ThatId NextId() => new(Guid.NewGuid());
    }

    public class ThisIdGenerator : IIdGenerator<ThisId>
    {
        private int _nextId = 1;
        public ThisId NextId() => new(_nextId++);
    }
}