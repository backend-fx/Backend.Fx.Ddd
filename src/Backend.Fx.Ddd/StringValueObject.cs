using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

/// <summary>
/// Any string that has a meaning in the domain model should be modeled as a value object.
/// This base class gives us some benefits:
/// <list type="bullet">
/// <item>No struggling between empty string and null strings, since the value cannot be <c>null</c>.</item>
/// <item>Automatic validation of min. and max. length</item>
/// <item>Automatic trimming</item>
/// <item>Convenience methods to create string value objects from plain CLR strings, applying optional and mandatory business rules</item>
/// </list>
/// </summary>
[PublicAPI]
[DebuggerDisplay("{DebuggerDisplay}")]
public abstract class StringValueObject : ValueObject, IComparable<StringValueObject>
{
    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    protected StringValueObject(string value, int minLength = 0, int maxLength = int.MaxValue)
    {
        if (value == null)
        {
            throw new ArgumentNullException($"{GetType().Name} Value cannot be null.", nameof(value));
        }

        if (minLength < 0)
        {
            throw new ArgumentOutOfRangeException($"{GetType().Name} minLength cannot be negative.", nameof(minLength));
        }

        if (maxLength < minLength)
        {
            throw new ArgumentOutOfRangeException(
                $"{GetType().Name} minLength cannot be smaller than the min lenght or negative.", nameof(minLength));
        }

        value = value.Trim();

        if (value.Length < minLength)
        {
            throw new ArgumentException($"{GetType().Name} value cannot be shorter than {minLength}.", nameof(value));
        }

        if (value.Length > maxLength)
        {
            throw new ArgumentException($"{GetType().Name} value cannot be longer than {maxLength}.", nameof(value));
        }

        Value = value;
    }

    private string DebuggerDisplay => $"{GetType().Name}: {Value}";

    public string Value { get; }

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public int Length => Value.Length;

    public static T Empty<T>() where T : StringValueObject
    {
        return string.Empty.AsOptionalValue<T>();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(StringValueObject svo)
    {
        return svo.Value;
    }

    public int CompareTo(StringValueObject other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return string.Compare(Value, other.Value, StringComparison.Ordinal);
    }
}
