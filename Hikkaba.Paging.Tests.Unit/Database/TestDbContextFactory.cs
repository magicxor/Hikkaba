using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Paging.Tests.Unit.Database;

public static class TestDbContextFactory
{
    public static async Task<TestDbContext> GetTestDbContextAsync(string dbName)
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        builder.UseInMemoryDatabase(dbName);

        var db = new TestDbContext(builder.Options);
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        return db;
    }
}
