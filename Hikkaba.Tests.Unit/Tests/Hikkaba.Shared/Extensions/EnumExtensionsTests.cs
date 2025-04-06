using Hikkaba.Shared.Extensions;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Tests.Unit.Tests.Hikkaba.Shared.Extensions;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class EnumExtensionsTests
{
    internal enum TestValues
    {
        Zero = 0,
        One = 1,
        Ten = 10,
        MinusOne = -1,
    }

    [TestCase(TestValues.Zero, 0)]
    [TestCase(TestValues.One, 1)]
    [TestCase(TestValues.Ten, 10)]
    [TestCase(TestValues.MinusOne, -1)]
    public void ToInt_ShouldReturnCorrectIntegerValue(TestValues value, int expected)
    {
        Assert.That(value.ToInt(), Is.EqualTo(expected));
    }

    [TestCase(TestValues.Zero, 0, "Zero")]
    [TestCase(TestValues.One, 1, "One")]
    [TestCase(TestValues.Ten, 10, "Ten")]
    [TestCase(TestValues.MinusOne, -1, "MinusOne")]
    public void ToEventId_ShouldReturnCorrectEventId(TestValues value, int expectedId, string expectedName)
    {
        var expectedEventId = new EventId(expectedId, expectedName);
        var actualEventId = value.ToEventId();

        Assert.That(actualEventId.Id, Is.EqualTo(expectedEventId.Id));
        Assert.That(actualEventId.Name, Is.EqualTo(expectedEventId.Name));
        // We can also compare the struct directly if needed, though comparing fields is often clearer
        Assert.That(actualEventId, Is.EqualTo(expectedEventId));
    }
}
