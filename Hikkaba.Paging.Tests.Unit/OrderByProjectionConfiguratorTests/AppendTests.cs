using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Hikkaba.Paging.OrderByProjection;
using Hikkaba.Paging.Tests.Unit.Database;
using Hikkaba.Paging.Tests.Unit.Filters;
using Hikkaba.Paging.Tests.Unit.Models;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Paging.Tests.Unit.OrderByProjectionConfiguratorTests;

[TestFixture]
public sealed class AppendTests
{
    private const string DbName = "AppendTests";

    [SetUp]
    public void SetUp()
    {
        OrderByProjectionConfig
            .ForType<Person>()
            .Append(nameof(Person.PersonId), OrderByDirection.Asc);
    }

    [TearDown]
    public void TearDown()
    {
        OrderByProjectionConfig.Clear();
    }

    [Test]
    public void ParallelConfigure_WithMultipleThreads_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            var taskNumbers = Enumerable.Range(1, 100);
            Parallel.ForEach(taskNumbers, _ =>
            {
                OrderByProjectionConfig
                    .ForType<Person>()
                    .Append(nameof(Person.PersonId), OrderByDirection.Asc);
            });
        });
    }

    [Test]
    public void ApplyOrderByWithFallback_WithSingleOrderBy_ReturnsOrderedQuery()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { PersonId = 3, Name = "Lee", Surname = "Smith", Age = 30 },
            new() { PersonId = 2, Name = "Lee", Surname = "Smith", Age = 30 },
            new() { PersonId = 1, Name = "Lee", Surname = "Smith", Age = 35 },
            new() { PersonId = 4, Name = "Lee", Surname = "Johnson", Age = 25 },
            new() { PersonId = 5, Name = "Johnny", Surname = "Smith", Age = 20 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(Person.Name), Direction = OrderByDirection.Asc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter, o => o.PersonId).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(5));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].PersonId, Is.EqualTo(5));
            Assert.That(result[1].PersonId, Is.EqualTo(1));
            Assert.That(result[2].PersonId, Is.EqualTo(2));
            Assert.That(result[3].PersonId, Is.EqualTo(3));
            Assert.That(result[4].PersonId, Is.EqualTo(4));
        });
        Assert.That(result, Is
            .Ordered.Ascending.By(nameof(Person.Name))
            .Then.Ascending.By(nameof(Person.PersonId)));
    }

    [Test]
    public async Task ApplyOrderByWithFallback_WithSingleOrderBy_ReturnsOrderedQuery2()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "Lee", Surname = "Smith", Age = 30 },
            new() { Name = "Lee", Surname = "Jackson", Age = 30 },
            new() { Name = "Rayan", Surname = "Smith", Age = 35 },
            new() { Name = "Adam", Surname = "Johnson", Age = 25 },
            new() { Name = "Johnny", Surname = "Smith", Age = 20 },
        };

        await using var db = await TestDbContextFactory.GetTestDbContextAsync(DbName);
        await db.People.AddRangeAsync(objects);
        await db.SaveChangesAsync();

        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(Person.Name), Direction = OrderByDirection.Asc },
            },
        };

        // Act
        var result = await db.People.ApplyOrderBy(filter, o => o.PersonId).ToListAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(5));
        Assert.That(result, Is
            .Ordered.Ascending.By(nameof(Person.Name))
            .Then.Ascending.By(nameof(Person.PersonId)));
    }
}
