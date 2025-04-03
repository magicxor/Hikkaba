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
public class MapTests
{
    private const string DbName = "MapTests";

    [SetUp]
    public void SetUp()
    {
        OrderByProjectionConfig
            .ForType<Person>()
            .Map(nameof(PersonDto.FullName), entity => entity.Name)
            .Map(nameof(PersonDto.PersonAge), entity => entity.Age)
            .Map(nameof(PersonDto.GrossSalary), entity => entity.Salary)
            .Map(nameof(PersonDto.NameLength), entity => entity.Name.Length)
            .Map(nameof(PersonDto.SalaryDigits), entity => entity.Salary == 0 ? 1 : (entity.Salary > 0 ? 1 : 2) + (int)Math.Log10(Math.Abs((double)entity.Salary)));

        OrderByProjectionConfig
            .ForType<Company>()
            .Map(nameof(CompanyDto.CompanyId), entity => entity.CompanyId);

        OrderByProjectionConfig
            .ForType<PersonWithEmploymentDate>()
            .Map(nameof(PersonWithEmploymentDate.EmployedOn), entity => entity.EmployedOn.HasValue ? entity.EmployedOn.Value.AddDays(1) : entity.EmployedOn);
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
                    .Map(nameof(PersonDto.FullName), entity => entity.Name)
                    .Map(nameof(PersonDto.PersonAge), entity => entity.Age)
                    .Map(nameof(PersonDto.GrossSalary), entity => entity.Salary)
                    .Map(nameof(PersonDto.NameLength), entity => entity.Name.Length)
                    .Map(nameof(PersonDto.SalaryDigits), entity => entity.Salary == 0 ? 1 : (entity.Salary > 0 ? 1 : 2) + (int)Math.Log10(Math.Abs((double)entity.Salary)));
            });
        });
    }

    [Test]
    public void ApplyOrderByWithFallback_WithSingleOrderBy_ReturnsOrderedQuery()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "Lee", Age = 30 },
            new() { Name = "Rayan", Age = 35 },
            new() { Name = "Adam", Age = 25 },
            new() { Name = "Johnny", Age = 20 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(PersonDto.NameLength), Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter, o => o.Name).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Name, Is.EqualTo("Johnny"));
            Assert.That(result[1].Name, Is.EqualTo("Rayan"));
            Assert.That(result[2].Name, Is.EqualTo("Adam"));
            Assert.That(result[3].Name, Is.EqualTo("Lee"));
        });
    }

    [Test]
    public void ApplyOrderByWithFallback_WithSingleOrderBy_ReturnsOrderedQuery2()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "Lee", Age = 30 },
            new() { Name = "Rayan", Age = 35 },
            new() { Name = "Adam", Age = 25 },
            new() { Name = "Johnny", Age = 20 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(PersonDto.FullName), Direction = OrderByDirection.Asc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter, o => o.Name).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(4));
        Assert.That(result, Is.Ordered.Ascending.By(nameof(Person.Name)));
    }

    [Test]
    public async Task ApplyOrderByWithFallback_WithSingleOrderBy_ReturnsOrderedQuery3()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "Lee", Age = 30 },
            new() { Name = "Rayan", Age = 35 },
            new() { Name = "Adam", Age = 25 },
            new() { Name = "Johnny", Age = 20 },
        };

        await using var db = await TestDbContextFactory.GetTestDbContextAsync(DbName);
        await db.People.AddRangeAsync(objects);
        await db.SaveChangesAsync();

        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(PersonDto.NameLength), Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = await db.People.ApplyOrderBy(filter, o => o.Name).ToListAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Name, Is.EqualTo("Johnny"));
            Assert.That(result[1].Name, Is.EqualTo("Rayan"));
            Assert.That(result[2].Name, Is.EqualTo("Adam"));
            Assert.That(result[3].Name, Is.EqualTo("Lee"));
        });
    }

    [Test]
    public void ApplyOrderByWithFallback_WithMultipleOrderBy_ReturnsOrderedQuery()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "Lee", Age = 30, Salary = 99 },
            new() { Name = "Rayan", Age = 35, Salary = 833 },
            new() { Name = "Adam", Age = 25, Salary = 5 },
            new() { Name = "Johnny", Age = 20, Salary = 20000 },
        };
        var query = objects.AsQueryable();
        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(PersonDto.SalaryDigits), Direction = OrderByDirection.Asc },
                new OrderByItem { Field = nameof(PersonDto.NameLength), Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter, o => o.Name).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Name, Is.EqualTo("Adam"));
            Assert.That(result[1].Name, Is.EqualTo("Lee"));
            Assert.That(result[2].Name, Is.EqualTo("Rayan"));
            Assert.That(result[3].Name, Is.EqualTo("Johnny"));
        });
    }

    [Test]
    public async Task ApplyOrderByWithFallback_WithMultipleOrderBy_ReturnsOrderedQuery2()
    {
        // Arrange
        var objects = new List<Person>
        {
            new() { Name = "Lee", Age = 30, Salary = 99 },
            new() { Name = "Rayan", Age = 35, Salary = 833 },
            new() { Name = "Adam", Age = 25, Salary = 5 },
            new() { Name = "Johnny", Age = 20, Salary = 20000 },
        };

        await using var db = await TestDbContextFactory.GetTestDbContextAsync(DbName);
        await db.People.AddRangeAsync(objects);
        await db.SaveChangesAsync();

        IPagingFilter filter = new SearchPageBasedFilter
        {
            OrderBy = new[]
            {
                new OrderByItem { Field = nameof(PersonDto.SalaryDigits), Direction = OrderByDirection.Asc },
                new OrderByItem { Field = nameof(PersonDto.NameLength), Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = await db.People.ApplyOrderBy(filter, o => o.Name).ToListAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Name, Is.EqualTo("Adam"));
            Assert.That(result[1].Name, Is.EqualTo("Lee"));
            Assert.That(result[2].Name, Is.EqualTo("Rayan"));
            Assert.That(result[3].Name, Is.EqualTo("Johnny"));
        });
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
                new OrderByItem { Field = nameof(PersonWithEmploymentDate.EmployedOn), Direction = OrderByDirection.Desc },
            },
        };

        // Act
        var result = query.ApplyOrderBy(filter).ToList();

        // Assert
        Assert.That(result, Is.Ordered
            .Descending.By(nameof(PersonWithEmploymentDate.EmployedOn)));
    }
}
