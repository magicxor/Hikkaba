using Hikkaba.Paging.Constants;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Paging.Tests.Unit.Filters;

namespace Hikkaba.Paging.Tests.Unit.FilterTests;

[TestFixture]
public sealed class SkipBasedPagingFilterTests
{
    [Test]
    public void PageSize_WithDefaultValue_ReturnsDefaultPageSize()
    {
        // Arrange
        var filter = new SearchSkipBasedFilter();

        // Act & Assert
        Assert.That(filter.PageSize, Is.EqualTo(Defaults.PageSize));
    }

    [Test]
    public void SkipCount_WithDefaultValue_ReturnsDefaultSkipCount()
    {
        // Arrange
        var filter = new SearchSkipBasedFilter();

        // Act & Assert
        Assert.That(filter.SkipCount, Is.EqualTo(Defaults.SkipCount));
    }

    [Test]
    public void OrderBy_WithDefaultValue_ReturnsNull()
    {
        // Arrange
        var filter = new SearchSkipBasedFilter();

        // Act & Assert
        Assert.That(filter.OrderBy, Is.Null);
    }

    [Test]
    public void GetPageSize_WithValidPageSize_ReturnsPageSize()
    {
        // Arrange
        var filter = new SearchSkipBasedFilter { PageSize = 10 };

        // Act & Assert
        Assert.That(filter.GetPageSize(), Is.EqualTo(10));
    }

    [Test]
    public void GetSkipCount_WithValidSkipCount_ReturnsSkipCount()
    {
        // Arrange
        var filter = new SearchSkipBasedFilter { SkipCount = 20 };

        // Act & Assert
        Assert.That(filter.GetSkipCount(), Is.EqualTo(20));
    }

    [Test]
    public void GetPageNumber_WithValidSkipCountAndPageSize_ReturnsPageNumber()
    {
        // Arrange
        var filter = new SearchSkipBasedFilter { SkipCount = 20, PageSize = 10 };

        // Act & Assert
        Assert.That(filter.GetPageNumber(), Is.EqualTo(3));
    }

    [Test]
    public void GetOrderBy_WithValidOrderBy_ReturnsOrderBy()
    {
        // Arrange
        var orderBy = new List<OrderByItem>
        {
            new()
            {
                Field = "Name",
                Direction = OrderByDirection.Asc,
            },
        };
        var filter = new SearchSkipBasedFilter { OrderBy = orderBy };

        // Act & Assert
        Assert.That(filter.GetOrderBy(), Is.EqualTo(orderBy));
    }
}
