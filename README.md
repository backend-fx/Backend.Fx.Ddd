# `Backend.Fx.Ddd`: Domain Driven Design Building Blocks

[![NuGet version (Backend.Fx.Ddd)](https://img.shields.io/nuget/v/Backend.Fx.Ddd.svg?style=flat-square)](https://www.nuget.org/packages/Backend.Fx.Ddd/)


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
| `StringValueObject` | Base class for modelling textual properties as type safe string values. |
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
