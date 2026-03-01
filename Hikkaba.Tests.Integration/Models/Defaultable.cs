using JetBrains.Annotations;

namespace Hikkaba.Tests.Integration.Models;

/// <summary>
/// <para>
/// Represents a value that can be set to a default (= left unset).
/// This is useful for scenarios where you want to distinguish between
/// a value that has been explicitly set to null and a value that has not been set at all.
/// </para>
///
/// <para>
/// Possible states:
///
/// <list type="number">
///    <item>
///        <term><b>default</b></term>
///        <description> - undefined (skipped by user). <b>HasValue is false</b></description>
///    </item>
///    <item>
///        <term>explicitly set to <b>null</b></term>
///        <description> - user explicitly set the value to null. <b>HasValue is true</b>, Value is null</description>
///    </item>
///    <item>
///        <term>explicitly set to <b>non-null</b> value</term>
///        <description> - user explicitly set the value to a non-null value. <b>HasValue is true</b>, Value is non-null</description>
///    </item>
/// </list>
/// </para>
/// </summary>
/// <typeparam name="T">Type of the value.</typeparam>
/// <exception cref="InvalidOperationException">Thrown when attempting to access Value without it being explicitly set.</exception>
public readonly record struct Defaultable<T>
{
    /// <summary>
    /// Gets the value associated with the object if it has been explicitly set.
    /// Accessing this property throws an <see cref="InvalidOperationException"/> if the value has not been set.
    /// Use <see cref="HasValue"/> to check if the value has been set before accessing this property.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to access the value without it being explicitly set.
    /// </exception>
    [PublicAPI]
    public readonly T Value => HasValue
        ? field
        : throw new InvalidOperationException("Cannot access Value because it has not been explicitly set.");

    /// <summary>
    /// Gets a value indicating whether the value has been explicitly set.
    /// </summary>
    [PublicAPI]
    public readonly bool HasValue { get; }

    [PublicAPI]
    public Defaultable(T value)
    {
        Value = value;
        HasValue = true;
    }

    /// <summary>
    /// Attempts to retrieve the value if it has been explicitly set.
    /// </summary>
    /// <param name="value">When this method returns, contains the value if it was explicitly set; otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the value has been explicitly set; otherwise, <c>false</c>.</returns>
    [PublicAPI]
    public bool TryGetValue(out T? value)
    {
        if (HasValue)
        {
            value = Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Implicitly converts a value to a <see cref="Defaultable{T}"/> instance.
    /// </summary>
    /// <param name="value">The value to convert. May be <c>null</c>.</param>
    /// <returns>A <see cref="Defaultable{T}"/> instance with the provided value.</returns>
    [PublicAPI]
    public static implicit operator Defaultable<T>(T value) => new(value);

    public Defaultable<T> ToDefaultable()
    {
        return this;
    }
}
