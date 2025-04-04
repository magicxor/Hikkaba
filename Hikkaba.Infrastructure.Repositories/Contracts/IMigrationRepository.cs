namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface IMigrationRepository
{
    Task<bool> MigrateAsync(CancellationToken cancellationToken);
}
