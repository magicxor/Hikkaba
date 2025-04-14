namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface ISeedRepository
{
    Task<bool> SeedAsync(CancellationToken cancellationToken);
}
