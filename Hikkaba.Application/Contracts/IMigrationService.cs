namespace Hikkaba.Application.Contracts;

public interface IMigrationService
{
    Task<bool> MigrateAsync(string key, CancellationToken cancellationToken);
}
