using System;
using Hikkaba.Shared.Extensions;

namespace Hikkaba.Tests.Unit.Tests.Hikkaba.Shared.Extensions;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class StringExtensionsTests
{
    [Test]
    public void TryLeft_WhenNull_ShouldReturnNull()
    {
        const string? source = null;
        var result = source.TryLeft(1);
        Assert.That(result, Is.Null);
    }

    [TestCase("", 0, "")]
    [TestCase("", 1, "")]
    [TestCase("a", 0, "")]
    [TestCase("a", 1, "a")]
    [TestCase("a", 2, "a")]
    [TestCase("a", 3, "a")]
    [TestCase("abc", 0, "")]
    [TestCase("abc", 1, "a")]
    [TestCase("abc", 2, "ab")]
    [TestCase("abc", 3, "abc")]
    [TestCase("abc", 4, "abc")]
    [TestCase("abc", 999, "abc")]
    public void TryLeft_WhenArgumentsValid_ShouldReturnExpectedResult(string source, int maxLength, string expectedResult)
    {
        var actualResult = source.TryLeft(maxLength);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase("", -1)]
    [TestCase("a", -2)]
    public void TryLeft_WhenMaxLengthIsLessThanZero_ShouldThrowArgumentOutOfRangeException(string source, int maxLength)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = source.TryLeft(maxLength));
    }

    [Test]
    public void TryRight_WhenNull_ShouldReturnNull()
    {
        const string? source = null;
        var result = source.TryRight(1);
        Assert.That(result, Is.Null);
    }

    [TestCase("", 0, "")]
    [TestCase("", 1, "")]
    [TestCase("a", 0, "")]
    [TestCase("a", 1, "a")]
    [TestCase("a", 2, "a")]
    [TestCase("a", 3, "a")]
    [TestCase("abc", 0, "")]
    [TestCase("abc", 1, "c")]
    [TestCase("abc", 2, "bc")]
    [TestCase("abc", 3, "abc")]
    [TestCase("abc", 4, "abc")]
    [TestCase("abc", 999, "abc")]
    public void TryRight_WhenArgumentsValid_ShouldReturnExpectedResult(string source, int maxLength, string expectedResult)
    {
        var actualResult = source.TryRight(maxLength);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase("", -1)]
    [TestCase("a", -2)]
    public void TryRight_WhenMaxLengthIsLessThanZero_ShouldThrowArgumentOutOfRangeException(string source, int maxLength)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = source.TryRight(maxLength));
    }

    [TestCase(null, null, true)]
    [TestCase(null, "", false)]
    [TestCase("", null, false)]
    [TestCase("", "", true)]
    [TestCase("test", "TEST", true)]
    [TestCase("test", "test", true)]
    [TestCase("test", "other", false)]
    public void EqualsIgnoreCase_ShouldReturnExpectedResult(string? source, string? target, bool expectedResult)
    {
        var actualResult = source.EqualsIgnoreCase(target);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void GetNonEmpty_WhenAllValuesEmpty_ShouldReturnEmptyCollection()
    {
        var result = StringExtensions.FilterNonEmpty(null, string.Empty, " ", "\t", "\n");
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetNonEmpty_WhenSomeValuesNonEmpty_ShouldReturnNonEmptyValues()
    {
        var result = StringExtensions.FilterNonEmpty(null, "test1", string.Empty, " ", "test2", "\t", "\n");
        Assert.That(result, Is.EqualTo(new[] { "test1", "test2" }));
    }

    [Test]
    public void IsNotNullOrEmpty_WhenNull_ShouldReturnFalse()
    {
        const string? value = null;
        Assert.That(value.IsNotNullOrEmpty(), Is.False);
    }

    [Test]
    public void IsNotNullOrEmpty_WhenEmpty_ShouldReturnFalse()
    {
        Assert.That(string.Empty.IsNotNullOrEmpty(), Is.False);
    }

    [Test]
    public void IsNotNullOrEmpty_WhenNonEmpty_ShouldReturnTrue()
    {
        Assert.That("test".IsNotNullOrEmpty(), Is.True);
    }

    [Test]
    public void Cut_WhenNull_ShouldReturnNull()
    {
        const string? source = null;
        var result = source!.Cut(10);
        Assert.That(result, Is.Null);
    }

    [TestCase("", 0, "")]
    [TestCase("", 10, "")]
    [TestCase("abc", 10, "abc")] // Length > text length
    [TestCase("abcdef", 6, "abcdef")] // Length == text length
    [TestCase("abcdefghij", 10, "abcdefghij")] // Length == text length
    [TestCase("abcdefghijk", 10, "abcdefg...")] // Length < text length
    [TestCase("1234", 4, "1234")] // Length == text length
    [TestCase("12345", 4, "1...")] // Length < text length, length = 4
    [TestCase("1234", 3, "...")] // Length < text length, length = 3
    public void Cut_WhenTextLengthNotExceedsLengthOrLengthIs3_ShouldReturnExpected(string source, int length, string expected)
    {
        var result = source.Cut(length);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("abc", 0)]
    [TestCase("abc", 1)]
    [TestCase("abc", 2)]
    public void Cut_WhenLengthIsLessThan3_ShouldThrowArgumentOutOfRangeException(string source, int length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = source.Cut(length));
    }
}
