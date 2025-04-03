using System.Globalization;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Paging.Tests.Unit.Filters;
using Hikkaba.Paging.Tests.Unit.TypeDescriptorContexts;
using Hikkaba.Paging.TypeConverters;

namespace Hikkaba.Paging.Tests.Unit;

[TestFixture]
public class OrderByTypeConverterTests
{
    [Test]
    public void CanConvertFrom_WithStringType_ReturnsTrue()
    {
        // Arrange
        var converter = new OrderByTypeConverter();
        var context = new TestTypeDescriptorContext(new SearchSkipBasedFilter(), nameof(SearchSkipBasedFilter.OrderBy));

        // Act
        var result = converter.CanConvertFrom(context, typeof(string));

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void CanConvertFrom_WithOtherType_ReturnsFalse()
    {
        // Arrange
        var converter = new OrderByTypeConverter();
        var context = new TestTypeDescriptorContext(new SearchSkipBasedFilter(), nameof(SearchSkipBasedFilter.OrderBy));

        // Act
        var result = converter.CanConvertFrom(context, typeof(int));

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void ConvertFrom_WithValidString_ReturnsOrderByItem1()
    {
        // Arrange
        var converter = new OrderByTypeConverter();
        var context = new TestTypeDescriptorContext(new SearchSkipBasedFilter(), nameof(SearchSkipBasedFilter.OrderBy));
        const string value = "Name,desc";

        // Act
        var result = converter.ConvertFrom(context, CultureInfo.InvariantCulture, value);

        // Assert
        Assert.That(result, Is.InstanceOf<OrderByItem>());
        if (result is OrderByItem orderByItem)
        {
            Assert.Multiple(() =>
            {
                Assert.That(orderByItem.Field, Is.EqualTo("Name"));
                Assert.That(orderByItem.Direction, Is.EqualTo(OrderByDirection.Desc));
            });
        }
    }

    [Test]
    public void ConvertFrom_WithValidString_ReturnsOrderByItem2()
    {
        // Arrange
        var converter = new OrderByTypeConverter();
        var context = new TestTypeDescriptorContext(new SearchSkipBasedFilter(), nameof(SearchSkipBasedFilter.OrderBy));
        const string value = "Name";

        // Act
        var result = converter.ConvertFrom(context, CultureInfo.InvariantCulture, value);

        // Assert
        Assert.That(result, Is.InstanceOf<OrderByItem>());
        if (result is OrderByItem orderByItem)
        {
            Assert.Multiple(() =>
            {
                Assert.That(orderByItem.Field, Is.EqualTo("Name"));
                Assert.That(orderByItem.Direction, Is.EqualTo(OrderByDirection.Asc));
            });
        }
    }
}
