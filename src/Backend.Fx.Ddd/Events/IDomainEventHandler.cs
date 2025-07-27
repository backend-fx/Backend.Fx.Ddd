using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Ddd.Events;

public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : class
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}