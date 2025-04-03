using System.Globalization;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Tests.Unit.Database;
using Hikkaba.Paging.Tests.Unit.Models;

namespace Hikkaba.Paging.Tests.Unit;

[TestFixture]
public sealed class ExpressionExtensionsTests
{
    private const string DbName = "ExpressionExtensionsTests";
    private readonly IReadOnlyCollection<int> _collection = new List<int> { 10, 4, 8, 1, 7, 0, 0, 2, 5 };

    [Test]
    public void OrderMethodExists_ReturnsTrue_WhenOrderByExists()
    {
        var query = _collection.AsQueryable().OrderBy(x => x);
        Assert.That(query.Expression.OrderMethodExists(), Is.True);
    }

    [Test]
    public void OrderMethodExists_ReturnsTrue_WhenOrderByDescendingExists1()
    {
        var query = _collection.AsQueryable().OrderByDescending(x => x);
        Assert.That(query.Expression.OrderMethodExists(), Is.True);
    }

    [Test]
    public async Task OrderMethodExists_ReturnsTrue_WhenOrderByDescendingExists2()
    {
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30 },
            new() { Name = "Jane", Age = 25 },
            new() { Name = "Bob", Age = 35 },
        };

        await using var db = await TestDbContextFactory.GetTestDbContextAsync(DbName);
        await db.People.AddRangeAsync(objects);
        await db.SaveChangesAsync();

        var query = db.People.OrderByDescending(x => x.Age);
        Assert.That(query.Expression.OrderMethodExists(), Is.True);
    }

    [Test]
    public void OrderMethodExists_ReturnsTrue_WhenOrderBySelectExists()
    {
        var query = _collection.AsQueryable()
            .OrderBy(x => x)
            .Select(x => x.ToString(CultureInfo.InvariantCulture))
            .Select(x => x.FirstOrDefault());
        Assert.That(query.Expression.OrderMethodExists(), Is.True);
    }

    [Test]
    public void OrderMethodExists_ReturnsTrue_WhenOrderByDescendingSelectExists()
    {
        var query = _collection.AsQueryable()
            .Select(x => new { Num = x, Letter = "a" })
            .OrderByDescending(x => x.Num)
            .Select(x => x.Num.ToString(CultureInfo.InvariantCulture))
            .Select(x => x.FirstOrDefault());
        Assert.That(query.Expression.OrderMethodExists(), Is.True);
    }

    [Test]
    public void OrderMethodExists_ReturnsFalse_WhenOrderByDoesNotExist1()
    {
        var query = _collection.AsQueryable()
            .Select(x => new { Num = x, Letter = "a" })
            .Select(x => x.Num.ToString(CultureInfo.InvariantCulture))
            .Select(x => x.FirstOrDefault());
        Assert.That(query.Expression.OrderMethodExists(), Is.False);
    }

    [Test]
    public void OrderMethodExists_ReturnsFalse_WhenOrderByDoesNotExist2()
    {
        var query = _collection.AsQueryable();
        Assert.That(query.Expression.OrderMethodExists(), Is.False);
    }

    [Test]
    public async Task OrderMethodExists_ReturnsTrue_WhenOrderByDoesNotExist3()
    {
        var objects = new List<Person>
        {
            new() { Name = "John", Age = 30 },
            new() { Name = "Jane", Age = 25 },
            new() { Name = "Bob", Age = 35 },
        };

        await using var db = await TestDbContextFactory.GetTestDbContextAsync(DbName);
        await db.People.AddRangeAsync(objects);
        await db.SaveChangesAsync();

        var query = db.People.AsQueryable();
        Assert.That(query.Expression.OrderMethodExists(), Is.False);
    }
}
