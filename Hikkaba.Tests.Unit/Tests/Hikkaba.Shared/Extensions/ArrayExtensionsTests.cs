using System;
using Hikkaba.Shared.Extensions;

namespace Hikkaba.Tests.Unit.Tests.Hikkaba.Shared.Extensions;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class ArrayExtensionsTests
{
    // Test cases for null inputs
    [Test]
    public void Compare_BothNull_ShouldReturnZero()
    {
        byte[]? array1 = null;
        byte[]? array2 = null;
        Assert.That(array1.Compare(array2), Is.EqualTo(0));
    }

    [Test]
    public void Compare_FirstNullSecondNotNull_ShouldReturnMinusOne()
    {
        byte[]? array1 = null;
        byte[] array2 = [1, 2];
        Assert.That(array1.Compare(array2), Is.EqualTo(-1));
    }

    [Test]
    public void Compare_FirstNotNullSecondNull_ShouldReturnOne()
    {
        byte[] array1 = [1, 2];
        byte[]? array2 = null;
        Assert.That(array1.Compare(array2), Is.EqualTo(1));
    }

    // Test cases using StructuralComparisons.StructuralComparer
    [TestCase(new byte[] { }, new byte[] { }, 0)] // Both empty
    [TestCase(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3 }, 0)] // Equal content and length
    [TestCase(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 4 }, -1)] // Same length, first < second
    [TestCase(new byte[] { 1, 2, 4 }, new byte[] { 1, 2, 3 }, 1)] // Same length, first > second
    public void Compare_ArraysWithSameLength_ShouldReturnExpectedStructuralComparison(byte[] array1, byte[] array2, int expected)
    {
        Assert.That(array1.Compare(array2), Is.EqualTo(expected));
    }

    [TestCase(new byte[] { 1 }, new byte[] { })] // First longer
    [TestCase(new byte[] { }, new byte[] { 1 })] // Second longer
    [TestCase(new byte[] { 1, 2 }, new byte[] { 1, 2, 3 })] // First is prefix of second
    [TestCase(new byte[] { 1, 2, 3 }, new byte[] { 1, 2 })] // Second is prefix of first
    public void Compare_ArraysWithDifferentLengths_ShouldThrowArgumentException(byte[] array1, byte[] array2)
    {
        Assert.Throws<ArgumentException>(() => _ = array1.Compare(array2));
    }
}
