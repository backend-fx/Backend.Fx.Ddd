using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd.Events;

[PublicAPI]
public class DomainEventOutBox : IEnumerable<IDomainEvent>
{
    private readonly HashSet<IDomainEvent> _events = new();

    public void Add(IDomainEvent domainEvent)
    {
        _events.Add(domainEvent);
    }

    public IEnumerator<IDomainEvent> GetEnumerator()
    {
        return _events.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_events).GetEnumerator();
    }
}
