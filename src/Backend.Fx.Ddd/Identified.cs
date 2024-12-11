using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

[PublicAPI]
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
public abstract class Identified<TId> : IEquatable<Identified<TId>>
{
    public TId Id { get; init; }

    /// <summary>
    /// DON'T USE!
    /// This ctor is only here to allow O/R-Mappers to materialize an object coming from a persistent
    /// store using reflection.
    /// </summary>
    [Obsolete("This ctor is only here to allow O/R-Mappers to materialize an object coming from a persistent")]
    protected Identified()
    {
        Id = default!;
    }

    protected Identified(TId id)
    {
        Id = id;
    }

    [UsedImplicitly] public string DebuggerDisplay => $"{GetType().Name}[{Id}]";

    public bool Equals(Identified<TId> other)
    {
        return !ReferenceEquals(other, null) && Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        var other = obj as Identified<TId>;
        return !ReferenceEquals(other, null) && Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Identified<TId>? left, Identified<TId>? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return true;
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;

        return ReferenceEquals(left, right) || right.Id.Equals(left.Id);

    }

    public static bool operator !=(Identified<TId>? left, Identified<TId>? right)
    {
        return !(left == right);
    }
}
