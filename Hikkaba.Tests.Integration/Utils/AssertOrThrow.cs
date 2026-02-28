using System.Diagnostics.CodeAnalysis;

namespace Hikkaba.Tests.Integration.Utils;

public static class AssertOrThrow
{
    /// <summary>
    /// Use this method instead of Assert.IsNotNull() to avoid the compiler warning
    /// "CS8602: Dereference of a possibly null reference".
    /// <br />
    /// Verifies that the object that is passed in is not equal to <see langword="null"/>.
    /// </summary>
    /// <param name="anObject">The object that is to be tested</param>
    [SuppressMessage("Roslynator", "RCS1256:Invalid argument null check", Justification = "False positive because of NUnit suppressor")]
    public static void IsNotNull([NotNull] object? anObject)
    {
        Assert.That(anObject, Is.Not.Null);
        ArgumentNullException.ThrowIfNull(anObject);
    }
}
