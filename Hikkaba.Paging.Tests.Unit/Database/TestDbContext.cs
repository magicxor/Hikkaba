using Hikkaba.Paging.Tests.Unit.Models;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Paging.Tests.Unit.Database;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> People { get; set; } = null!;
}
