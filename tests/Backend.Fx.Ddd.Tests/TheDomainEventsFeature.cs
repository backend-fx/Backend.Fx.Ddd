using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Ddd.Events;
using Backend.Fx.Ddd.Events.Feature;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using FakeItEasy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Fx.Ddd.Tests;

public class TheDomainEventsFeature : IAsyncLifetime
{
    private readonly IBackendFxApplication _app;

    public TheDomainEventsFeature()
    {
        _app = new TestApplication(GetType().Assembly);
        _app.EnableFeature(new DomainEventsFeature());
    }

    public async Task InitializeAsync()
    {
        await _app.BootAsync();
    }

    [Fact]
    public void RaisedEventIsHandledOnCompleteOfOperation()
    {
        var domainEvent = new TestEvent();

        _app.Invoker.InvokeAsync((sp, _) =>
        {
            sp.GetRequiredService<IDomainEventPublisher>().PublishDomainEvent(domainEvent);

            // handling is postponed to the completion of the operation
            A.CallTo(() => TestEventHandler.Fake.HandleAsync(domainEvent, A<CancellationToken>._))
                .MustNotHaveHappened();

            return Task.CompletedTask;
        });

        // now it must be handled
        A.CallTo(() => TestEventHandler.Fake.HandleAsync(domainEvent, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        // this one was not touched
        A.CallTo(() => UnusedTestEventHandler.Fake.HandleAsync(A<UnusedTestEvent>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    public class TestEvent : IDomainEvent;

    [UsedImplicitly]
    public class TestEventHandler : IDomainEventHandler<TestEvent>
    {
        public static readonly IDomainEventHandler<TestEvent> Fake = A.Fake<IDomainEventHandler<TestEvent>>();

        public async Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await Fake.HandleAsync(domainEvent, cancellationToken);
        }
    }

    [UsedImplicitly]
    public class UnusedTestEvent : IDomainEvent;

    [UsedImplicitly]
    public class UnusedTestEventHandler : IDomainEventHandler<UnusedTestEvent>
    {
        public static readonly IDomainEventHandler<UnusedTestEvent> Fake = A.Fake<IDomainEventHandler<UnusedTestEvent>>();

        public async Task HandleAsync(UnusedTestEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await Fake.HandleAsync(domainEvent, cancellationToken);
        }
    }

    private class TestApplication(params Assembly[] assemblies)
        : BackendFxApplication(
            new SimpleInjectorCompositionRoot(),
            new DebugExceptionLogger(),
            assemblies);

    public Task DisposeAsync()
    {
        _app.Dispose();
        return Task.CompletedTask;
    }
}