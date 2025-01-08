namespace Backend.Fx.Ddd.Events;

public interface IHaveDomainEvents
{
    DomainEventOutBox DomainEvents { get; }
}