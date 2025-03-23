using System;
using System.Diagnostics.CodeAnalysis;

namespace Hikkaba.Tests.Integration.Utils;

public static class AssertThrow
{
    /// <summary>
    /// Use this method instead of Assert.IsNotNull() to avoid the compiler warning
    /// "CS8602: Dereference of a possibly null reference".
    /// <br />
    /// Verifies that the object that is passed in is not equal to <see langword="null"/>.
    /// </summary>
    /// <param name="anObject">The object that is to be tested</param>
    public static void IsNotNull([NotNull] object? anObject)
    {
        Assert.That(anObject, Is.Not.Null);
        ArgumentNullException.ThrowIfNull(anObject);
    }
}
