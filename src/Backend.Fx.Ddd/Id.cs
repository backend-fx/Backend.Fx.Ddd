using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

public abstract class Id
{
    private static readonly ConcurrentDictionary<Type, string> TypeNameCache = new();
    protected static string GetTypeName(Type idType)
    {
        return TypeNameCache.GetOrAdd(idType, t =>
        {
            string idTypeName = t.Name;
            if (idTypeName.EndsWith("Id"))
            {
                idTypeName = idTypeName.Substring(0, idTypeName.Length - 2);
            }

            return idTypeName;
        });
    }
}

[PublicAPI]
public abstract class Id<T> : Id, IEquatable<Id<T>> where T : struct, IEquatable<T>
{
     

    public T Value { get; }

    protected Id(T value)
    {
        if (value.Equals(default))
        {
            throw new ArgumentException($"The {GetTypeName(GetType())} ID value must be specified.", nameof(value));
        }

        Value = value;
    }

    public bool Equals(Id<T>? other)
    {
        return other is not null && Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Id<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return $"{GetTypeName(GetType())}/{Value}";
    }

    public static bool operator ==(Id<T>? left, Id<T>? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Id<T>? left, Id<T>? right)
    {
        return !(left == right);
    }
}

[PublicAPI]
public abstract class IntId : Id<int>
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
public abstract class LongId : Id<long>
{
    protected LongId(long value) : base(value)
    {
        if (value < 0)
        {
            throw new ArgumentException("The ID value must be non-negative.", nameof(value));
        }
    }
}