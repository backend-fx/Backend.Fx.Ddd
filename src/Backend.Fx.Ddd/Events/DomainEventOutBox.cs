using System.Collections;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd.Events;

/// <summary>
///     Collects the domain events raised by an entity until they are published. Events are kept in the order they
///     were added and are not deduplicated, so that value based events (e.g. records) raised multiple times are
///     handled multiple times.
/// </summary>
[PublicAPI]
public class DomainEventOutBox : IEnumerable<object>
{
    private readonly List<object> _events = new();

    public int Count => _events.Count;

    public void Add(object domainEvent)
    {
        _events.Add(domainEvent);
    }

    /// <summary>
    ///     Removes all collected domain events. This is called by the <see cref="IDomainEventPublisher" /> after
    ///     publishing, so that a subsequent publication does not raise the same events again.
    /// </summary>
    public void Clear()
    {
        _events.Clear();
    }

    /// <summary>
    ///     Returns all collected domain events and empties the out box in a single operation.
    /// </summary>
    public IReadOnlyList<object> Drain()
    {
        var drained = _events.ToArray();
        _events.Clear();
        return drained;
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
