using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

[PublicAPI]
public abstract class Id
{
    private static readonly ConcurrentDictionary<Type, string> TypeNameCache = new();

    protected static string GetTypeName(Type idType)
    {
        return TypeNameCache.GetOrAdd(idType, t =>
        {
            string idTypeName = t.Name;

            // a type that is literally named "Id" keeps its name, stripping the suffix would leave an empty string
            if (idTypeName.Length > 2 && idTypeName.EndsWith("Id", StringComparison.Ordinal))
            {
                idTypeName = idTypeName.Substring(0, idTypeName.Length - 2);
            }

            return idTypeName;
        });
    }
}

/// <summary>
///     Base class for strongly typed ids. The self referencing type parameter <typeparamref name="TSelf" /> makes the
///     concrete id type implement <see cref="IEquatable{T}" /> of itself, so that it can be used as the id type of an
///     <see cref="IAggregateRoot{TId}" />.
/// </summary>
/// <typeparam name="TSelf">The concrete id type deriving from this class.</typeparam>
/// <typeparam name="TValue">The type of the wrapped primitive value.</typeparam>
[PublicAPI]
public abstract class Id<TSelf, TValue> : Id, IEquatable<TSelf>
    where TSelf : Id<TSelf, TValue>
    where TValue : struct, IEquatable<TValue>
{
    protected Id(TValue value)
    {
        if (value.Equals(default))
        {
            throw new ArgumentException($"The {GetTypeName(GetType())} ID value must be specified.", nameof(value));
        }

        Value = value;
    }

    public TValue Value { get; }

    public bool Equals(TSelf? other)
    {
        return other is not null && other.GetType() == GetType() && Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is TSelf other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return GetType().GetHashCode() * 397 ^ Value.GetHashCode();
        }
    }

    public override string ToString()
    {
        return $"{GetTypeName(GetType())}/{Value}";
    }

    public static bool operator ==(Id<TSelf, TValue>? left, Id<TSelf, TValue>? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right as TSelf);
    }

    public static bool operator !=(Id<TSelf, TValue>? left, Id<TSelf, TValue>? right)
    {
        return !(left == right);
    }
}

[PublicAPI]
public abstract class IntId<TSelf> : Id<TSelf, int> where TSelf : IntId<TSelf>
{
    protected IntId(int value) : base(value)
    {
        if (value < 0)
        {
            throw new ArgumentException("The ID value must be non-negative.", nameof(value));
        }
    }
}

[PublicAPI]
public abstract class LongId<TSelf> : Id<TSelf, long> where TSelf : LongId<TSelf>
{
    protected LongId(long value) : base(value)
    {
        if (value < 0)
        {
            throw new ArgumentException("The ID value must be non-negative.", nameof(value));
        }
    }
}

[PublicAPI]
public abstract class GuidId<TSelf> : Id<TSelf, Guid> where TSelf : GuidId<TSelf>
{
    protected GuidId(Guid value) : base(value)
    {
    }
}
