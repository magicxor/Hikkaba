using Hikkaba.Paging.Models;
using Hikkaba.Paging.Tests.Unit.Filters;

namespace Hikkaba.Paging.Tests.Unit;

[TestFixture]
public class PagedResultTests
{
    [Test]
    public void Constructor_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyList<object> data = null!;
        var filter = new SearchPageBasedFilter();

        // Act & Assert
        Assert.That(() => new PagedResult<object>(data, filter), Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_WithNullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        // ReSharper disable once CollectionNeverUpdated.Local
        var data = new List<object>();
        IPagingFilter filter = null!;

        // Act & Assert
        Assert.That(() => new PagedResult<object>(data, filter), Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_WithValidDataAndFilter_SetsProperties()
    {
        // Arrange
        var data = new List<object> { new(), new(), new() };
        var filter = new SearchPageBasedFilter { PageNumber = 2, PageSize = 1 };

        // Act
        var result = new PagedResult<object>(data, filter, 3);

        // Assert
        Assert.That(result.Data, Is.EqualTo(data));
        Assert.That(result.PageSize, Is.EqualTo(1));
        Assert.That(result.SkippedItemCount, Is.EqualTo(1));
        Assert.That(result.PageNumber, Is.EqualTo(2));
        Assert.That(result.TotalPageCount, Is.EqualTo(3));
        Assert.That(result.TotalItemCount, Is.EqualTo(3));
        Assert.That(result.GetFilter(), Is.EqualTo(filter));
    }

    [Test]
    public void TotalPageCount_WithNullTotalItemCount_ReturnsNull()
    {
        // Arrange
        // ReSharper disable once CollectionNeverUpdated.Local
        var data = new List<object>();
        var filter = new SearchPageBasedFilter();

        // Act
        var result = new PagedResult<object>(data, filter);

        // Assert
        Assert.That(result.TotalPageCount, Is.Null);
    }

    [Test]
    public void TotalPageCount_WithValidTotalItemCount_ReturnsTotalPageCount()
    {
        // Arrange
        // ReSharper disable once CollectionNeverUpdated.Local
        var data = new List<string>
        {
            "a",
            "b",
        };
        var filter = new SearchPageBasedFilter { PageSize = 2 };
        const int totalItemCount = 5;

        // Act
        var result = new PagedResult<object>(data, filter, totalItemCount);

        // Assert
        Assert.That(result.TotalPageCount, Is.EqualTo(3));
    }
}
