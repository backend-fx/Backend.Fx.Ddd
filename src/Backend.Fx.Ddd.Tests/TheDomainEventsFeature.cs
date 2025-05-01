using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Ddd.Events;
using Backend.Fx.Ddd.Feature;
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
    public async Task RaisedEventIsHandledOnCompleteOfOperation()
    {
        var domainEvent = new TestEvent1();

        await _app.Invoker.InvokeAsync((sp, _) =>
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

    [Fact]
    public async Task RaisedEventIsHandledOnCompleteOfOperationUsingOutboxPattern()
    {
        var domainEvent = new TestEvent1();

        await _app.Invoker.InvokeAsync((sp, _) =>
        {
            var entity = new EntityWithDomainEvents();
            entity.DomainEvents.Add(domainEvent);

            sp.GetRequiredService<IDomainEventPublisher>().PublishDomainEvents(entity);

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

    [Fact]
    public async Task SingleHandlerCanHandleMultipleEvents()
    {
        var domainEvent2 = new TestEvent2();
        var domainEvent3 = new TestEvent3();

        await _app.Invoker.InvokeAsync((sp, _) =>
        {
            sp.GetRequiredService<IDomainEventPublisher>().PublishDomainEvent(domainEvent2);
            sp.GetRequiredService<IDomainEventPublisher>().PublishDomainEvent(domainEvent3);

            // handling is postponed to the completion of the operation
            A.CallTo(() => MultiTestEventHandler.Fake2.HandleAsync(domainEvent2, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => MultiTestEventHandler.Fake3.HandleAsync(domainEvent3, A<CancellationToken>._))
                .MustNotHaveHappened();

            return Task.CompletedTask;
        });

        // now it must be handled
        A.CallTo(() => MultiTestEventHandler.Fake2.HandleAsync(domainEvent2, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => MultiTestEventHandler.Fake3.HandleAsync(domainEvent3, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }


    public class EntityWithDomainEvents : IHaveDomainEvents
    {
        public DomainEventOutBox DomainEvents { get; } = new();
    }

    public class TestEvent1 : IDomainEvent;

    public class TestEvent2 : IDomainEvent;

    public class TestEvent3 : IDomainEvent;

    [UsedImplicitly]
    public class TestEventHandler : IDomainEventHandler<TestEvent1>
    {
        public static readonly IDomainEventHandler<TestEvent1> Fake = A.Fake<IDomainEventHandler<TestEvent1>>();

        public async Task HandleAsync(TestEvent1 domainEvent1, CancellationToken cancellationToken = default)
        {
            await Fake.HandleAsync(domainEvent1, cancellationToken);
        }
    }

    [UsedImplicitly]
    public class MultiTestEventHandler : IDomainEventHandler<TestEvent2>, IDomainEventHandler<TestEvent3>
    {
        public static readonly IDomainEventHandler<TestEvent2> Fake2 = A.Fake<IDomainEventHandler<TestEvent2>>();
        public static readonly IDomainEventHandler<TestEvent3> Fake3 = A.Fake<IDomainEventHandler<TestEvent3>>();

        public async Task HandleAsync(TestEvent2 domainEvent, CancellationToken cancellationToken = default)
        {
            await Fake2.HandleAsync(domainEvent, cancellationToken);
        }

        public async Task HandleAsync(TestEvent3 domainEvent, CancellationToken cancellationToken = default)
        {
            await Fake3.HandleAsync(domainEvent, cancellationToken);
        }
    }

    [UsedImplicitly]
    public class UnusedTestEvent : IDomainEvent;

    [UsedImplicitly]
    public class UnusedTestEventHandler : IDomainEventHandler<UnusedTestEvent>
    {
        public static readonly IDomainEventHandler<UnusedTestEvent> Fake =
            A.Fake<IDomainEventHandler<UnusedTestEvent>>();

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