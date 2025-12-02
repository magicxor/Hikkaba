using System;
using Hikkaba.Data.Entities;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    private ApplicationUser? _admin;

    public ApplicationUser Admin => _admin ?? throw new InvalidOperationException("Admin not created. Call WithDefaultAdmin() first.");

    public TestDataBuilder WithDefaultAdmin()
    {
        _admin = new ApplicationUser
        {
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = "896e8014-c237-41f5-a925-dabf640ee4c4",
            ConcurrencyStamp = "43035b63-359d-4c23-8812-29bbc5affbf2",
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
        };
        _dbContext.Users.Add(_admin);
        return this;
    }

    private void EnsureAdminExists()
    {
        if (_admin == null)
        {
            throw new InvalidOperationException("Admin must be created first. Call WithDefaultAdmin().");
        }
    }
}
