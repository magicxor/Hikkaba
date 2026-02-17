using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Hikkaba.Shared.Extensions;

/// <summary>
/// Extension methods for string.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Checks if the string is not null or empty.
    /// </summary>
    /// <param name="text">source string</param>
    /// <returns>True if the string is not null or empty, false otherwise.</returns>
    [Pure]
    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? text)
    {
        return !string.IsNullOrEmpty(text);
    }

    /// <summary>
    /// Checks if the string is null or empty.
    /// </summary>
    /// <param name="text">source string</param>
    /// <returns>True if the string is null or empty, false otherwise.</returns>
    [Pure]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? text)
    {
        return string.IsNullOrEmpty(text);
    }

    /// <summary>
    /// Returns the leftmost maxLength characters from the string.
    /// </summary>
    /// <param name="text">source string</param>
    /// <param name="maxLength">maximum length of the string</param>
    /// <returns>Leftmost maxLength characters from the string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is less than 0.</exception>
    [Pure]
    [return: NotNullIfNotNull(nameof(text))]
    public static string? TakeLeft(this string? text, int maxLength)
    {
        if (text is null)
        {
            return null;
        }

        return maxLength switch
        {
            0 => string.Empty,
            < 0 => throw new ArgumentOutOfRangeException(nameof(maxLength), $"{nameof(maxLength)} must be greater than or equal to 0"),
            _ => text.Length <= maxLength ? text : text[..maxLength],
        };
    }

    /// <summary>
    /// Returns the rightmost maxLength characters from the string.
    /// </summary>
    /// <param name="text">source string</param>
    /// <param name="maxLength">maximum length of the string</param>
    /// <returns>Rightmost maxLength characters from the string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is less than 0.</exception>
    [Pure]
    [return: NotNullIfNotNull(nameof(text))]
    public static string? TakeRight(this string? text, int maxLength)
    {
        if (text is null)
        {
            return null;
        }

        return maxLength switch
        {
            0 => string.Empty,
            < 0 => throw new ArgumentOutOfRangeException(nameof(maxLength), $"{nameof(maxLength)} must be greater than or equal to 0"),
            _ => text.Length <= maxLength ? text : text[^maxLength..],
        };
    }

    /// <summary>
    /// Compares two strings ignoring case.
    /// </summary>
    /// <param name="source">source string</param>
    /// <param name="target">target string</param>
    /// <returns>True if the strings are equal, false otherwise.</returns>
    [Pure]
    public static bool EqualsIgnoreCase(this string? source, string? target)
    {
        return string.Equals(source, target, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Cuts the string to the specified length and appends "..." if it exceeds that length.
    /// </summary>
    /// <param name="text">source string</param>
    /// <param name="maxLength">maximum length of the string</param>
    /// <returns>Cut string if it exceeds the specified length, otherwise returns the original string.</returns>
    [Pure]
    [return: NotNullIfNotNull(nameof(text))]
    public static string? Cut(this string? text, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 0);

        const string ellipsis = "...";
        if (!string.IsNullOrEmpty(text) && text.Length > maxLength)
        {
            if (maxLength <= ellipsis.Length)
            {
                return text.AsSpan(0, maxLength).ToString();
            }

            text = string.Concat(text.AsSpan(0, maxLength - ellipsis.Length), ellipsis);
        }

        return text;
    }

    [Pure]
    public static IReadOnlyCollection<string> WhereNotNullOrWhiteSpace(params string?[] values)
    {
        return values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToList()
            .AsReadOnly();
    }
}
