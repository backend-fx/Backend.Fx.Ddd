using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

[PublicAPI]
public abstract class ComparableValueObject : ValueObject, IComparable
{
    public int CompareTo(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return 0;
        }

        if (ReferenceEquals(null, obj))
        {
            return 1;
        }

        if (obj is not ComparableValueObject valueObject)
        {
            throw new InvalidOperationException();
        }

        return CompareTo(valueObject);
    }

    protected abstract IEnumerable<IComparable> GetComparableComponents();

    protected int CompareTo(ComparableValueObject? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (ReferenceEquals(null, other))
        {
            return 1;
        }

        using var thisComponents = GetComparableComponents().GetEnumerator();
        using var otherComponents = other.GetComparableComponents().GetEnumerator();
        while (true)
        {
            var x = thisComponents.MoveNext();
            var y = otherComponents.MoveNext();
            if (x != y)
            {
                throw new InvalidOperationException();
            }

            if (x)
            {
                var c = thisComponents.Current?.CompareTo(otherComponents.Current) ?? 0;
                if (c != 0)
                {
                    return c;
                }
            }
            else
            {
                break;
            }
        }

        return 0;
    }
}

[PublicAPI]
public abstract class ComparableValueObject<T> : ComparableValueObject, IComparable<T>
    where T : ComparableValueObject<T>
{
    public int CompareTo(T? other)
    {
        return base.CompareTo(other);
    }
}
