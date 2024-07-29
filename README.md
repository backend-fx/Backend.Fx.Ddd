# `Backend.Fx.Ddd`: Domain Driven Design Building Blocks

Provides basic types as foundation for a strongly typed and interpretation free domain model.

### Type overview

| Type | Usage |
|---|---|
| `ComparableValueObject` | Base class for a value object that supports [comparison](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/comparison-operators). |
| `IAggregateRoot` | Marks the root of an aggregate. |
| `Identified` | Base class for anything that has a conceptual identity. The type of the identity value is a generic type parameter, so it can be a primitive type like `int`, `long`, `string` `Guid` or a strong typed identifier like a `CustomerId` that derives from `ValueObject`. |
| `IDomainService` | Marks a service as domain service. When using the `DomainServicesFeature` in a `BackendFxApplication`, all domain service types are auto wired with a scoped lifetime. |
| `IIdGenerator` | Defines the generator of strongly typed identity values |
| `IRepository` | Defines a minimal repository that retrieves `IAggregateRoot` instances. |
| `ValueObject` | Base class for anything that has attributes but lacks a conceptual identity value. Instances are considered equal when all attributes are equal. |

### Usage

1. Add a dependency to `Backend.Fx.Ddd` in your domain assembly
1. Add a dependency to `Backend.Fx.Ddd.Feature` in your composition assembly (that's the assembly where your `BackendFxApplication` lives)
1. register the feature in the application

```csharp
public class MyCoolApplication 
{
    public MyCoolApplication() : base(new SimpleInjectorCompositionRoot(), new ExceptionLogger(), GetAssemblies())
    {
        CompositionRoot.RegisterModules( ... );

        EnableFeature(new DomainServicesFeature());
    }
}
``` 

# `Backend.Fx.Ddd.Events`

A simple, yet powerful implementation of the _Domain Events_ pattern. Domain events can be raised from inside any domain service, and can be handled by zero, one or many independent handlers. Those events are handled inside the same transactional boundary and a failing handler will result in a roll back.

### Type overview

| Type | Usage |
|---|---|
| `IDomainEvent` | Marks a domain event |
| `IDomainEventHandler` | Marks a handler for a domain event. Handlers are called in no specific order  before the `IOperation` completes. When using the `DomainEventsFeature` in a `BackendFxApplication`, all domain event handlers are auto wired with a scoped lifetime. |
| `IDomainEventPublisher` | Defines the API to publish domain events from a domain service. When using the `DomainEventsFeature` in a `BackendFxApplication`, a suitable implementation will be injected to the requesting service. |

### Usage

1. Add a dependency to `Backend.Fx.Ddd.Events` in your domain assembly
1. Add a dependency to `Backend.Fx.Ddd.Events.Feature` in your composition assembly (that's the assembly where your `BackendFxApplication` lives)
1. register the feature in the application

```csharp
public class MyCoolApplication 
{
    public MyCoolApplication() : base(new SimpleInjectorCompositionRoot(), new ExceptionLogger(), GetAssemblies())
    {
        CompositionRoot.RegisterModules( ... );

        EnableFeature(new DomainEventsFeature());
    }
}
``` 
