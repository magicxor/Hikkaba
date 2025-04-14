using Hikkaba.Paging.Constants;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Paging.Tests.Unit.Filters;

namespace Hikkaba.Paging.Tests.Unit.FilterTests;

[TestFixture]
public sealed class PageBasedPagingFilterTests
{
    [Test]
    public void PageSize_WithDefaultValue_ReturnsDefaultPageSize()
    {
        // Arrange
        var filter = new SearchPageBasedFilter();

        // Act & Assert
        Assert.That(filter.PageSize, Is.EqualTo(Defaults.PageSize));
    }

    [Test]
    public void PageNumber_WithDefaultValue_ReturnsDefaultPageNumber()
    {
        // Arrange
        var filter = new SearchPageBasedFilter();

        // Act & Assert
        Assert.That(filter.PageNumber, Is.EqualTo(Defaults.PageNumber));
    }

    [Test]
    public void OrderBy_WithDefaultValue_ReturnsNull()
    {
        // Arrange
        var filter = new SearchPageBasedFilter();

        // Act & Assert
        Assert.That(filter.OrderBy, Is.Null);
    }

    [Test]
    public void GetPageSize_WithValidPageSize_ReturnsPageSize()
    {
        // Arrange
        var filter = new SearchPageBasedFilter { PageSize = 10 };

        // Act & Assert
        Assert.That(filter.GetPageSize(), Is.EqualTo(10));
    }

    [Test]
    public void GetSkipCount_WithValidPageNumberAndPageSize_ReturnsSkipCount()
    {
        // Arrange
        var filter = new SearchPageBasedFilter { PageNumber = 3, PageSize = 10 };

        // Act & Assert
        Assert.That(filter.GetSkipCount(), Is.EqualTo(20));
    }

    [Test]
    public void GetPageNumber_WithValidPageNumber_ReturnsPageNumber()
    {
        // Arrange
        var filter = new SearchPageBasedFilter { PageNumber = 3 };

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
        var filter = new SearchPageBasedFilter { OrderBy = orderBy };

        // Act & Assert
        Assert.That(filter.GetOrderBy(), Is.EqualTo(orderBy));
    }
}
