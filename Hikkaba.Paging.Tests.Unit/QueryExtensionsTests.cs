using System.Linq.Expressions;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Hikkaba.Paging.Tests.Unit.Database;
using Hikkaba.Paging.Tests.Unit.Filters;
using Hikkaba.Paging.Tests.Unit.Models;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Paging.Tests.Unit;

[TestFixture]
public class QueryExtensionsTests
{
    private const string DbName = "QueryExtensionsTests";

    [Test]
    public void ApplyOrderBy_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<object>? query = null;
        IPagingFilter filter = new SearchPageBasedFilter();

        // Act & Assert
        Assert.That(() => query!.ApplyOrderBy(filter), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyOrderBy_WithNullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        var query = Enumerable.Empty<object>().AsQueryable();
        IPagingFilter filter = null!;

        // Act & Assert
        Assert.That(() => query.ApplyOrderBy(filter), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyOrderBy_WithEmptyOrderBy_ThrowsArgumentException1()
    {
        // Arrange
        var query = new List<Person> { new(), new(), new() }.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter();

        // Act & Assert
        Assert.That(() => query.ApplyOrderBy(filter), Throws.ArgumentException);
    }

    [Test]
    public void ApplyOrderBy_WithEmptyOrderBy_ThrowsArgumentException2()
    {
        // Arrange
        var query = new List<Person> { new(), new(), new() }.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new List<OrderByItem>(),
        };

        // Act & Assert
        Assert.That(() => query.ApplyOrderBy(filter), Throws.ArgumentException);
    }

    [Test]
    public void ApplyOrderBy_WithEmptyOrderBy_ThrowsArgumentException3()
    {
        // Arrange
        var query = new List<Person> { new(), new(), new() }.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new List<OrderByItem>
            {
                new()
                {
                    Field = string.Empty,
                    Direction = OrderByDirection.Asc,
                },
            },
        };

        // Act & Assert
        Assert.That(() => query.ApplyOrderBy(filter), Throws.ArgumentException);
    }

    [Test]
    public void ApplyOrderBy_WithSingleOrderBy_ReturnsOrderedQuery1()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30 },
            new() { Name = "Jane", Age = 25 },
            new() { Name = "Bob", Age = 35 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem
                {
                    Field = nameof(Person.Age),
                    Direction = OrderByDirection.Desc,
                },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter).ToList();

        // Assert
        Assert.That(result, Is.Ordered.Descending.By(nameof(Person.Age)));
    }

    [Test]
    public async Task ApplyOrderBy_WithSingleOrderBy_ReturnsOrderedQuery2()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30 },
            new() { Name = "Jane", Age = 25 },
            new() { Name = "Bob", Age = 35 },
        };

        await using var db = await TestDbContextFactory.GetTestDbContextAsync(DbName);
        await db.People.AddRangeAsync(objects);
        await db.SaveChangesAsync();

        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem
                {
                    Field = nameof(Person.Age),
                    Direction = OrderByDirection.Desc,
                },
            },
        };

        // Act
        var result = await db.People.ApplyOrderBy(filter).ToListAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(objects.Count));
        Assert.That(result, Is.Ordered.Descending.By(nameof(Person.Age)));
    }

    [Test]
    public void ApplyOrderBy_WithMultipleOrderBy_ReturnsOrderedQuery1()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000, Grade = 4 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Bob", Age = 35, Salary = 40000, Grade = 2 },
            new() { Name = "Maria", Age = 29, Salary = 50000, Grade = 20 },
            new() { Name = "Maria", Age = 27, Salary = 30000 },
            new() { Name = "Bob", Age = 31, Salary = 40000 },
            new() { Name = "Megan", Age = 26, Salary = 50000 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(Person.Salary), Direction = OrderByDirection.Asc },
                new OrderByItem { Field = nameof(Person.Age), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(Person.Grade), Direction = OrderByDirection.Asc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter).ToList();

        // Assert
        Assert.That(result, Is.Ordered
            .Ascending.By(nameof(Person.Salary))
            .Then.Descending.By(nameof(Person.Age))
            .Then.Ascending.By(nameof(Person.Grade)));
    }

    [Test]
    public void ApplyOrderBy_WithMultipleOrderBy_ReturnsOrderedQuery2()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000, Grade = 2 },
            new() { Name = "Jane", Age = 25, Salary = 60000, Grade = 14 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
            new() { Name = "Maria", Age = 29, Salary = 50000 },
            new() { Name = "Maria", Age = 27, Salary = 30000, Grade = 10 },
            new() { Name = "Bob", Age = 31, Salary = 40000 },
            new() { Name = "Megan", Age = 26, Salary = 50000, Grade = 1 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(Person.Salary), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(Person.Age), Direction = OrderByDirection.Asc },
                new OrderByItem { Field = nameof(Person.Grade), Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter).ToList();

        // Assert
        Assert.That(result, Is.Ordered
            .Descending.By(nameof(Person.Salary))
            .Then.Ascending.By(nameof(Person.Age))
            .Then.Descending.By(nameof(Person.Grade)));
    }

    [Test]
    public void ApplyOrderBy_WithMultipleOrderBy_ReturnsOrderedQuery3()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
            new() { Name = "Maria", Age = 29, Salary = 50000 },
            new() { Name = "Maria", Age = 27, Salary = 30000 },
            new() { Name = "Bob", Age = 31, Salary = 40000 },
            new() { Name = "Megan", Age = 26, Salary = 50000 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(Person.Salary), Direction = OrderByDirection.Asc },
                new OrderByItem { Field = string.Empty, Direction = OrderByDirection.Asc },
                new OrderByItem { Field = nameof(Person.Age), Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter).ToList();

        // Assert
        Assert.That(result, Is.Ordered.Ascending.By(nameof(Person.Salary)).Then.Descending.By(nameof(Person.Age)));
    }

    [Test]
    public void ApplyOrderBy_WithMultipleOrderBy_ReturnsOrderedQuery4()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
            new() { Name = "Maria", Age = 29, Salary = 50000 },
            new() { Name = "Maria", Age = 27, Salary = 30000 },
            new() { Name = "Bob", Age = 31, Salary = 40000 },
            new() { Name = "Megan", Age = 26, Salary = 50000 },
        };
        var query = objects.AsQueryable();
        ISortingFilter filter = new SearchSortingFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(Person.Salary), Direction = OrderByDirection.Asc },
                new OrderByItem { Field = string.Empty, Direction = OrderByDirection.Asc },
                new OrderByItem { Field = nameof(Person.Age), Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter).ToList();

        // Assert
        Assert.That(result, Is.Ordered.Ascending.By(nameof(Person.Salary)).Then.Descending.By(nameof(Person.Age)));
    }

    [Test]
    public void ApplyOrderByWithFallback_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<object>? query = null;
        IPagingFilter filter = new SearchPageBasedFilter();
        Expression<Func<object, object>> fallbackOrderByKeySelector = o => o;

        // Act & Assert
        Assert.That(() => query!.ApplyOrderBy(filter, fallbackOrderByKeySelector), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyOrderByWithFallback_WithNullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        var query = Enumerable.Empty<object>().AsQueryable();
        IPagingFilter filter = null!;
        Expression<Func<object, object>> fallbackOrderByKeySelector = o => o;

        // Act & Assert
        Assert.That(() => query.ApplyOrderBy(filter, fallbackOrderByKeySelector), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyOrderByWithFallback_WithNullFallbackOrderByKeySelector_ThrowsArgumentNullException()
    {
        // Arrange
        var query = Enumerable.Empty<object>().AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter();
        Expression<Func<object, object>> fallbackOrderByKeySelector = null!;

        // Act & Assert
        Assert.That(() => query.ApplyOrderBy(filter, fallbackOrderByKeySelector), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyOrderByWithFallback_WithEmptyOrderBy_ReturnsFallbackOrderBy()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30 },
            new() { Name = "Jane", Age = 25 },
            new() { Name = "Bob", Age = 35 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter();

        // Act
        var result = query.ApplyOrderBy(filter, o => o.Age).ToList();

        // Assert
        Assert.That(result, Is.Ordered.Ascending.By(nameof(Person.Age)));
    }

    [Test]
    public void ApplyOrderByWithFallback_WithSingleOrderBy_ReturnsOrderedQuery()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30 },
            new() { Name = "Jane", Age = 25 },
            new() { Name = "Bob", Age = 35 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter { OrderBy = new[] { new OrderByItem { Field = "Age", Direction = OrderByDirection.Desc } } };

        // Act
        var result = query.ApplyOrderBy(filter, o => o.Name).ToList();

        // Assert
        Assert.That(result, Is.Ordered.Descending.By(nameof(Person.Age)));
    }

    [Test]
    public void ApplyOrderByWithFallback_WithMultipleOrderBy_ReturnsOrderedQuery()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = "Salary", Direction = OrderByDirection.Asc },
                new OrderByItem { Field = "Age", Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter, o => o.Name).ToList();

        // Assert
        Assert.That(result, Is.Ordered.Ascending.By(nameof(Person.Salary)).Then.Descending.By(nameof(Person.Age)));
    }

    [Test]
    public async Task ApplyOrderByWithFallback_WithMultipleOrderBy_ReturnsOrderedQuery2()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
        };

        await using var db = await TestDbContextFactory.GetTestDbContextAsync(DbName);
        await db.People.AddRangeAsync(objects);
        await db.SaveChangesAsync();

        var query = db.People;
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = "Salary", Direction = OrderByDirection.Asc },
                new OrderByItem { Field = "Age", Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = await query.ApplyOrderBy(filter, o => o.Name).ToListAsync();

        // Assert
        Assert.That(result, Is.Ordered.Ascending.By(nameof(Person.Salary)).Then.Descending.By(nameof(Person.Age)));
    }

    [Test]
    public async Task ApplyOrderByWithFallback_WithMultipleOrderByAndPaging_ReturnsOrderedQuery2()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
            new() { Name = "Maria", Age = 29, Salary = 20000 },
            new() { Name = "Mark", Age = 27, Salary = 30000 },
            new() { Name = "Mike", Age = 31, Salary = 10000 },
            new() { Name = "Megan", Age = 26, Salary = 70000 },
            new() { Name = "Molly", Age = 28, Salary = 80000 },
            new() { Name = "Mia", Age = 32, Salary = 90000 },
            new() { Name = "Mila", Age = 33, Salary = 100000 },
            new() { Name = "Marta", Age = 34, Salary = 110000 },
            new() { Name = "Maggie", Age = 36, Salary = 120000 },
            new() { Name = "Molly", Age = 37, Salary = 130000 },
            new() { Name = "Ivan", Age = 38, Salary = 140000 },
            new() { Name = "Oleg", Age = 39, Salary = 150000 },
            new() { Name = "Sergey", Age = 40, Salary = 160000 },
            new() { Name = "Vladimir", Age = 41, Salary = 170000 },
            new() { Name = "Vladislav", Age = 42, Salary = 180000 },
        };

        await using var db = await TestDbContextFactory.GetTestDbContextAsync(DbName);
        await db.People.AddRangeAsync(objects);
        await db.SaveChangesAsync();

        var query = db.People;
        IPagingFilter filter = new SearchPageBasedFilter
        {
            PageSize = 2,
            PageNumber = 3,
            OrderBy = new[]
            {
                new OrderByItem { Field = "Name", Direction = OrderByDirection.Asc },
                new OrderByItem { Field = "Salary", Direction = OrderByDirection.Asc },
            },
        };

        // Act
        var result = await query.ApplyOrderBy(filter, o => o.Name).ToListAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(objects.Count));
            Assert.That(result, Is.Ordered.Ascending.By(nameof(Person.Name)).Then.Ascending.By(nameof(Person.Salary)));
            Assert.That(filter.GetPageNumber(), Is.EqualTo(3));
        });
    }

    [Test]
    public void ApplyPaging_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<object> query = null!;
        IPagingFilter filter = new SearchPageBasedFilter();

        // Act & Assert
        Assert.That(() => query.ApplyPaging(filter), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyPaging_WithNullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        var query = Enumerable.Empty<object>().AsQueryable();
        IPagingFilter filter = null!;

        // Act & Assert
        Assert.That(() => query.ApplyPaging(filter), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyPaging_WithValidFilter_ReturnsPagedQuery()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30 },
            new() { Name = "Jane", Age = 25 },
            new() { Name = "Bob", Age = 35 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter { PageNumber = 2, PageSize = 1 };

        // Act
        var result = query.ApplyPaging(filter).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Jane"));
        Assert.That(filter.GetPageNumber(), Is.EqualTo(2));
    }

    [Test]
    public void ApplyOrderByAndPaging_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<object> query = null!;
        IPagingFilter filter = new SearchPageBasedFilter();

        // Act & Assert
        Assert.That(() => query.ApplyOrderByAndPaging(filter), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyOrderByAndPaging_WithNullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        var query = Enumerable.Empty<object>().AsQueryable();
        IPagingFilter filter = null!;

        // Act & Assert
        Assert.That(() => query.ApplyOrderByAndPaging(filter), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyOrderByAndPaging_WithValidFilter_ReturnsOrderedAndPagedQuery()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000 },
            new() { Name = "John", Age = 20, Salary = 10000 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Jane", Age = 21, Salary = 50000 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
            new() { Name = "Maria", Age = 29, Salary = 20000 },
            new() { Name = "Mark", Age = 27, Salary = 30000 },
            new() { Name = "Mike", Age = 31, Salary = 10000 },
            new() { Name = "Megan", Age = 26, Salary = 70000 },
            new() { Name = "Molly", Age = 28, Salary = 80000 },
            new() { Name = "Mia", Age = 32, Salary = 90000 },
            new() { Name = "Mila", Age = 33, Salary = 100000 },
            new() { Name = "Marta", Age = 34, Salary = 110000 },
            new() { Name = "Maggie", Age = 36, Salary = 120000 },
            new() { Name = "Molly", Age = 37, Salary = 130000 },
            new() { Name = "Ivan", Age = 38, Salary = 140000 },
            new() { Name = "Oleg", Age = 39, Salary = 150000 },
            new() { Name = "Sergey", Age = 40, Salary = 160000 },
            new() { Name = "Vladimir", Age = 41, Salary = 170000 },
            new() { Name = "Vladislav", Age = 42, Salary = 180000 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            PageNumber = 2,
            PageSize = 3,
            OrderBy = new[]
            {
                new OrderByItem { Field = "Name", Direction = OrderByDirection.Asc },
                new OrderByItem { Field = "Age", Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderByAndPaging(filter).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].Name, Is.EqualTo("Jane"));
            Assert.That(result[1].Name, Is.EqualTo("John"));
            Assert.That(result[2].Name, Is.EqualTo("John"));
            Assert.That(result[0].Salary, Is.EqualTo(50000));
            Assert.That(result[1].Salary, Is.EqualTo(50000));
            Assert.That(result[2].Salary, Is.EqualTo(10000));
            Assert.That(result, Is.Ordered.Ascending.By("Name").Then.Descending.By("Age"));
            Assert.That(filter.GetPageNumber(), Is.EqualTo(2));
        });
    }

    [Test]
    public void ApplyOrderByAndPagingWithFallback_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<object> query = null!;
        IPagingFilter filter = new SearchPageBasedFilter();
        Expression<Func<object, object>> fallbackOrderByKeySelector = o => o;

        // Act & Assert
        Assert.That(() => query.ApplyOrderByAndPaging(filter, fallbackOrderByKeySelector), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyOrderByAndPagingWithFallback_WithNullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        var query = Enumerable.Empty<object>().AsQueryable();
        IPagingFilter filter = null!;
        Expression<Func<object, object>> fallbackOrderByKeySelector = o => o;

        // Act & Assert
        Assert.That(() => query.ApplyOrderByAndPaging(filter, fallbackOrderByKeySelector), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyOrderByAndPagingWithFallback_WithNullFallbackOrderByKeySelector_ThrowsArgumentNullException()
    {
        // Arrange
        var query = Enumerable.Empty<object>().AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter();
        Expression<Func<object, object>> fallbackOrderByKeySelector = null!;

        // Act & Assert
        Assert.That(() => query.ApplyOrderByAndPaging(filter, fallbackOrderByKeySelector), Throws.ArgumentNullException);
    }

    [Test]
    public void ApplyOrderByAndPagingWithFallback_WithEmptyOrderBy_ReturnsFallbackOrderByAndPagedQuery()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
            new() { Name = "Maria", Age = 29, Salary = 20000 },
            new() { Name = "Mark", Age = 27, Salary = 30000 },
            new() { Name = "Mike", Age = 31, Salary = 10000 },
            new() { Name = "Megan", Age = 26, Salary = 70000 },
            new() { Name = "Molly", Age = 28, Salary = 80000 },
            new() { Name = "Mia", Age = 32, Salary = 90000 },
            new() { Name = "Mila", Age = 33, Salary = 100000 },
            new() { Name = "Marta", Age = 34, Salary = 110000 },
            new() { Name = "Maggie", Age = 36, Salary = 120000 },
            new() { Name = "Molly", Age = 37, Salary = 130000 },
            new() { Name = "Ivan", Age = 38, Salary = 140000 },
            new() { Name = "Oleg", Age = 39, Salary = 150000 },
            new() { Name = "Sergey", Age = 40, Salary = 160000 },
            new() { Name = "Vladimir", Age = 41, Salary = 170000 },
            new() { Name = "Vladislav", Age = 42, Salary = 180000 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            PageSize = 5,
            PageNumber = 2,
        };

        // Act
        var result = query.ApplyOrderByAndPaging(filter, x => x.Name, OrderByDirection.Desc).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(5));
        Assert.That(result, Is.Ordered.Descending.By("Name"));
        Assert.That(filter.GetPageNumber(), Is.EqualTo(2));
    }

    [Test]
    public void ApplyOrderByAndPagingWithFallback_WithSingleOrderBy_ReturnsOrderedAndPagedQuery()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
            new() { Name = "Maria", Age = 29, Salary = 20000 },
            new() { Name = "Mark", Age = 27, Salary = 30000 },
            new() { Name = "Mike", Age = 31, Salary = 10000 },
            new() { Name = "Megan", Age = 26, Salary = 70000 },
            new() { Name = "Molly", Age = 28, Salary = 80000 },
            new() { Name = "Mia", Age = 32, Salary = 90000 },
            new() { Name = "Mila", Age = 33, Salary = 100000 },
            new() { Name = "Marta", Age = 34, Salary = 110000 },
            new() { Name = "Maggie", Age = 36, Salary = 120000 },
            new() { Name = "Molly", Age = 37, Salary = 130000 },
            new() { Name = "Ivan", Age = 38, Salary = 140000 },
            new() { Name = "Oleg", Age = 39, Salary = 150000 },
            new() { Name = "Sergey", Age = 40, Salary = 160000 },
            new() { Name = "Vladimir", Age = 41, Salary = 170000 },
            new() { Name = "Vladislav", Age = 42, Salary = 180000 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            PageNumber = 3,
            PageSize = 5,
            OrderBy = new[] { new OrderByItem { Field = "Age", Direction = OrderByDirection.Desc } },
        };

        // Act
        var result = query.ApplyOrderByAndPaging(filter, x => x.Name).ToList();

        // Assert
        Assert.That(result, Is.Ordered.Descending.By("Age"));
        Assert.That(filter.GetPageNumber(), Is.EqualTo(3));
    }

    [Test]
    public void ApplyOrderByAndPagingWithFallback_WithMultipleOrderBy_ReturnsOrderedAndPagedQuery()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
            new() { Name = "Maria", Age = 29, Salary = 20000 },
            new() { Name = "Mark", Age = 27, Salary = 30000 },
            new() { Name = "Mike", Age = 31, Salary = 10000 },
            new() { Name = "Megan", Age = 26, Salary = 70000 },
            new() { Name = "Molly", Age = 28, Salary = 80000 },
            new() { Name = "Mia", Age = 32, Salary = 90000 },
            new() { Name = "Mila", Age = 33, Salary = 100000 },
            new() { Name = "Marta", Age = 34, Salary = 110000 },
            new() { Name = "Maggie", Age = 36, Salary = 120000 },
            new() { Name = "Molly", Age = 37, Salary = 130000 },
            new() { Name = "Ivan", Age = 38, Salary = 140000 },
            new() { Name = "Oleg", Age = 39, Salary = 150000 },
            new() { Name = "Sergey", Age = 40, Salary = 160000 },
            new() { Name = "Vladimir", Age = 41, Salary = 170000 },
            new() { Name = "Vladislav", Age = 42, Salary = 180000 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            PageNumber = 3,
            PageSize = 5,
            OrderBy = new[]
            {
                new OrderByItem { Field = "Age", Direction = OrderByDirection.Desc },
                new OrderByItem { Field = "Name", Direction = OrderByDirection.Asc },
                new OrderByItem { Field = "Salary", Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderByAndPaging(filter, x => x.Name).ToList();

        // Assert
        Assert.That(result, Is.Ordered.Descending.By("Age").Then.Ascending.By("Name").Then.Descending.By("Salary"));
        Assert.That(filter.GetPageNumber(), Is.EqualTo(3));
    }

    [Test]
    public void ApplyOrderByAndPagingWithFallback_WithMultipleOrderBy_ReturnsOrderedAndPagedQuery2()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30, Salary = 50000 },
            new() { Name = "Jane", Age = 25, Salary = 60000 },
            new() { Name = "Bob", Age = 35, Salary = 40000 },
            new() { Name = "Maria", Age = 29, Salary = 20000 },
            new() { Name = "Mark", Age = 27, Salary = 30000 },
            new() { Name = "Mike", Age = 31, Salary = 10000 },
            new() { Name = "Megan", Age = 26, Salary = 70000 },
            new() { Name = "Molly", Age = 28, Salary = 80000 },
            new() { Name = "Mia", Age = 32, Salary = 90000 },
            new() { Name = "Mila", Age = 33, Salary = 100000 },
            new() { Name = "Marta", Age = 34, Salary = 110000 },
            new() { Name = "Maggie", Age = 36, Salary = 120000 },
            new() { Name = "Molly", Age = 37, Salary = 130000 },
            new() { Name = "Ivan", Age = 38, Salary = 140000 },
            new() { Name = "Oleg", Age = 39, Salary = 150000 },
            new() { Name = "Sergey", Age = 40, Salary = 160000 },
            new() { Name = "Vladimir", Age = 41, Salary = 170000 },
            new() { Name = "Vladislav", Age = 42, Salary = 180000 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchSkipBasedFilter
        {
            SkipCount = 10,
            PageSize = 5,
            OrderBy = new[]
            {
                new OrderByItem { Field = "Age", Direction = OrderByDirection.Desc },
                new OrderByItem { Field = "Name", Direction = OrderByDirection.Asc },
                new OrderByItem { Field = "Salary", Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderByAndPaging(filter, x => x.Name).ToList();

        // Assert
        Assert.That(result, Is.Ordered.Descending.By("Age").Then.Ascending.By("Name").Then.Descending.By("Salary"));
        Assert.That(filter.GetPageNumber(), Is.EqualTo(3));
    }

    [Test]
    public void ApplyOrderBy_WithOrderByDateOnly_ReturnsOrderedQuery()
    {
        // Arrange
        var objects = new List<PersonWithEmploymentDate>
        {
            new() { Name = "John", EmployedOn = new DateOnly(2023, 01, 15) },
            new() { Name = "Jane", EmployedOn = new DateOnly(2023, 01, 17) },
            new() { Name = "Bob", EmployedOn = new DateOnly(2023, 01, 16) },
            new() { Name = "Maria", EmployedOn = new DateOnly(2023, 01, 15) },
            new() { Name = "Maria", EmployedOn = new DateOnly(2023, 01, 18) },
            new() { Name = "Bob", EmployedOn = new DateOnly(2023, 01, 01) },
            new() { Name = "Megan" },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(PersonWithEmploymentDate.Name), Direction = OrderByDirection.Asc },
                new OrderByItem { Field = nameof(PersonWithEmploymentDate.EmployedOn), Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter).ToList();

        // Assert
        Assert.That(result, Is.Ordered
            .Ascending.By(nameof(PersonWithEmploymentDate.Name))
            .Then.Descending.By(nameof(PersonWithEmploymentDate.EmployedOn)));
    }
}
