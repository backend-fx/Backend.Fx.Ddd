using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd.Events;

[PublicAPI]
public class DomainEventOutBox : IEnumerable<object>
{
    private readonly HashSet<object> _events = new();

    public void Add(object domainEvent)
    {
        _events.Add(domainEvent);
    }

    public IEnumerator<object> GetEnumerator()
    {
        return _events.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_events).GetEnumerator();
    }
}
