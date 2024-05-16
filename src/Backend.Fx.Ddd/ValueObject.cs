using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

/// <summary>
///     An object that contains attributes but has no conceptual identity.
/// </summary>
[PublicAPI]
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    ///     When overriden in a derived class, returns all components of a value objects which constitute its identity.
    /// </summary>
    /// <returns>An ordered list of equality components.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (ReferenceEquals(null, other)) return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (ReferenceEquals(null, obj)) return false;
        if (GetType() != obj.GetType()) return false;
        return GetEqualityComponents().SequenceEqual(((ValueObject) obj).GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (var obj in GetEqualityComponents())
            {
                hash = hash * 23 + (obj != null ? obj.GetHashCode() : 0);
            }

            return hash;
        }
    }
    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
