using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hikkaba.Application.Implementations;

public sealed class SeedService : ISeedService
{
    private readonly ILogger<SeedService> _logger;
    private readonly IOptions<HikkabaConfiguration> _options;
    private readonly ISeedRepository _seedRepository;

    public SeedService(
        ILogger<SeedService> logger,
        IOptions<HikkabaConfiguration> options,
        ISeedRepository seedRepository)
    {
        _logger = logger;
        _options = options;
        _seedRepository = seedRepository;
    }

    public async Task<bool> SeedAsync(string key, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seed called with key: {Key}", key);

        if (key != _options.Value.MaintenanceKey)
        {
            _logger.LogWarning("Invalid key provided for seeding: {Key}", key);
            return false;
        }

        return await _seedRepository.SeedAsync(cancellationToken);
    }
}
