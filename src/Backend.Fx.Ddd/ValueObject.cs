using System.Collections;
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
        if (ReferenceEquals(this, other))
            return true;
        if (ReferenceEquals(null, other))
            return false;
        if (GetType() != other.GetType())
            return false;

        return ComponentsEqual(GetEqualityComponents(), other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + GetType().GetHashCode();
            foreach (var component in GetEqualityComponents())
            {
                hash = hash * 23 + GetComponentHashCode(component);
            }

            return hash;
        }
    }

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);

    /// <summary>
    ///     Compares two sequences of equality components, applying structural comparison to components that are
    ///     collections themselves.
    /// </summary>
    private static bool ComponentsEqual(IEnumerable<object?> left, IEnumerable<object?> right)
    {
        using var leftEnumerator = left.GetEnumerator();
        using var rightEnumerator = right.GetEnumerator();

        while (true)
        {
            var hasLeft = leftEnumerator.MoveNext();
            var hasRight = rightEnumerator.MoveNext();

            if (hasLeft != hasRight)
                return false;
            if (!hasLeft)
                return true;
            if (!ComponentEquals(leftEnumerator.Current, rightEnumerator.Current))
                return false;
        }
    }

    private static bool ComponentEquals(object? left, object? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;

        // strings are enumerable, but must be compared as a whole
        if (left is string || right is string)
            return left.Equals(right);

        if (left is IEnumerable leftEnumerable && right is IEnumerable rightEnumerable)
        {
            return ComponentsEqual(leftEnumerable.Cast(), rightEnumerable.Cast());
        }

        return left.Equals(right);
    }

    private static int GetComponentHashCode(object? component)
    {
        switch (component)
        {
            case null:
                return 0;
            case string:
                return component.GetHashCode();
            case IEnumerable enumerable:
                unchecked
                {
                    var hash = 19;
                    foreach (var item in enumerable)
                    {
                        hash = hash * 23 + GetComponentHashCode(item);
                    }

                    return hash;
                }
            default:
                return component.GetHashCode();
        }
    }
}

internal static class NonGenericEnumerableExtensions
{
    public static IEnumerable<object?> Cast(this IEnumerable enumerable)
    {
        foreach (var item in enumerable)
        {
            yield return item;
        }
    }
}
