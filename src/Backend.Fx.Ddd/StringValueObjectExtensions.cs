using System;
using System.Collections.Concurrent;
using System.Reflection;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

[PublicAPI]
public static class StringValueObjectExtensions
{
    private static readonly ConcurrentDictionary<Type, ConstructorInfo> Constructors = new();

    /// <summary>
    ///     When the original string is null or empty, an empty string value object is returned,
    ///     otherwise the typed value object containing the original string is returned
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>A string value object ot type <c>T</c> containing the given string value or <c>string.Empty</c></returns>
    public static T AsOptionalValue<T>(this string? value) where T : StringValueObject
    {
        return Create<T>(value ?? string.Empty);
    }

    /// <summary>
    ///     When the original string is null or empty, an ArgumentException is thrown,
    ///     otherwise the typed value object containing the original string is returned
    /// </summary>
    /// <exception cref="ArgumentNullException">The string is null or empty</exception>
    public static T AsMandatoryValue<T>(this string? value) where T : StringValueObject
    {
        value = value?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value), $"{typeof(T).Name} cannot be null or empty.");
        }

        return Create<T>(value!);
    }

    /// <summary>
    ///     When the original string is null or empty, an error is added to the exception
    ///     builder, otherwise the typed value object containing the original string is returned
    /// </summary>
    public static T AsMandatoryValue<T>(this string? value, IExceptionBuilder exceptionBuilder)
        where T : StringValueObject
    {
        value = value?.Trim();
        exceptionBuilder.AddIf(string.IsNullOrEmpty(value), $"{typeof(T).Name} cannot be null or empty");
        return Create<T>(value ?? string.Empty);
    }

    private static T Create<T>(string value) where T : StringValueObject
    {
        var constructor = Constructors.GetOrAdd(
            typeof(T),
            t => t.GetConstructor(new[] { typeof(string) }) ??
                 throw new ArgumentException($"No constructor found for {t.Name} that accepts a string."));

        return (T)constructor.Invoke(new object[] { value });
    }
}
