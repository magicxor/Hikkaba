using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class MigrationRepository : IMigrationRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public MigrationRepository(
        ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<bool> MigrateAsync(CancellationToken cancellationToken)
    {
        await _applicationDbContext.Database.EnsureCreatedAsync(cancellationToken);

        if ((await _applicationDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            await _applicationDbContext.Database.MigrateAsync(cancellationToken);
        }

        return true;
    }
}
