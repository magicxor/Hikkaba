using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hikkaba.Application.Implementations;

public sealed class MigrationService : IMigrationService
{
    private readonly ILogger<MigrationService> _logger;
    private readonly IOptions<HikkabaConfiguration> _options;
    private readonly IMigrationRepository _migrationRepository;

    public MigrationService(
        ILogger<MigrationService> logger,
        IOptions<HikkabaConfiguration> options,
        IMigrationRepository migrationRepository)
    {
        _logger = logger;
        _options = options;
        _migrationRepository = migrationRepository;
    }

    public async Task<bool> MigrateAsync(string key, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrate called with key: {Key}", key);

        if (key != _options.Value.MaintenanceKey)
        {
            _logger.LogWarning("Invalid key provided for migration: {Key}", key);
            return false;
        }

        return await _migrationRepository.MigrateAsync(cancellationToken);
    }
}
