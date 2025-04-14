namespace Hikkaba.Application.Contracts;

public interface ISeedService
{
    Task<bool> SeedAsync(string key, CancellationToken cancellationToken);
}
