﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Fx.Ddd
{
    /// <summary>
    ///     An object that contains attributes but has no conceptual identity.
    ///     https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        ///     When overriden in a derived class, returns all components of a value objects which constitute its identity.
        /// </summary>
        /// <returns>An ordered list of equality components.</returns>
        protected abstract IEnumerable<object> GetEqualityComponents();

        public bool Equals(ValueObject other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;
            
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override bool Equals(object obj)
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
    }

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

            if (GetType() != obj.GetType())
            {
                throw new InvalidOperationException();
            }

            return CompareTo(obj as ComparableValueObject);
        }

        protected abstract IEnumerable<IComparable> GetComparableComponents();

        protected int CompareTo(ComparableValueObject other)
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

    public abstract class ComparableValueObject<T> : ComparableValueObject, IComparable<T>
        where T : ComparableValueObject<T>
    {
        public int CompareTo(T other)
        {
            return base.CompareTo(other);
        }
    }
}