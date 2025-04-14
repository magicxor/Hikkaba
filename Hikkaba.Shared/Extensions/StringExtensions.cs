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
    /// Checks if the string is null or empty.
    /// </summary>
    /// <param name="src">source string</param>
    /// <returns>True if the string is null or empty, false otherwise.</returns>
    [Pure]
    public static bool IsNotNullOrEmpty(this string? src)
    {
        return !string.IsNullOrEmpty(src);
    }

    /// <summary>
    /// Returns the leftmost maxLength characters from the string.
    /// </summary>
    /// <param name="src">source string</param>
    /// <param name="maxLength">maximum length of the string</param>
    /// <returns>Leftmost maxLength characters from the string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is less than 0.</exception>
    [Pure]
    [return: NotNullIfNotNull(nameof(src))]
    public static string? TryLeft(this string? src, int maxLength)
    {
        if (src is null)
        {
            return null;
        }

        return maxLength switch
        {
            0 => string.Empty,
            < 0 => throw new ArgumentOutOfRangeException(nameof(maxLength), $"{nameof(maxLength)} must be greater than 0"),
            _ => src.Length <= maxLength ? src : src[..maxLength],
        };
    }

    /// <summary>
    /// Returns the rightmost maxLength characters from the string.
    /// </summary>
    /// <param name="src">source string</param>
    /// <param name="maxLength">maximum length of the string</param>
    /// <returns>Rightmost maxLength characters from the string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is less than 0.</exception>
    [Pure]
    [return: NotNullIfNotNull(nameof(src))]
    public static string? TryRight(this string? src, int maxLength)
    {
        if (src is null)
        {
            return null;
        }

        return maxLength switch
        {
            0 => string.Empty,
            < 0 => throw new ArgumentOutOfRangeException(nameof(maxLength), $"{nameof(maxLength)} must be greater than 0"),
            _ => src.Length <= maxLength ? src : src[^maxLength..],
        };
    }

    /// <summary>
    /// Compares two strings ignoring case.
    /// </summary>
    /// <param name="src">source string</param>
    /// <param name="target">target string</param>
    /// <returns>True if the strings are equal, false otherwise.</returns>
    [Pure]
    public static bool EqualsIgnoreCase(this string? src, string? target)
    {
        return string.Equals(src, target, StringComparison.OrdinalIgnoreCase);
    }

    [Pure]
    public static IReadOnlyCollection<string> FilterNonEmpty(params string?[] values)
    {
        return values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToList();
    }

    [Pure]
    public static string Cut(this string text, int length)
    {
        if (!string.IsNullOrEmpty(text) && text.Length > length)
        {
            text = string.Concat(text.AsSpan(0, length - 3), "...");
        }

        return text;
    }
}
