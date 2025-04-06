using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Api;

[ApiController]
[AllowAnonymous]
[Route("api/v1/maintenance")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class MaintenanceController : ControllerBase
{
    private readonly IMigrationService _migrationService;
    private readonly ISeedService _seedService;

    public MaintenanceController(
        IMigrationService migrationService,
        ISeedService seedService)
    {
        _migrationService = migrationService;
        _seedService = seedService;
    }

    [HttpPost("migrate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<bool> Migrate(string key, CancellationToken cancellationToken)
    {
        return await _migrationService.MigrateAsync(key, cancellationToken);
    }

    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<bool> Seed(string key, CancellationToken cancellationToken)
    {
        return await _seedService.SeedAsync(key, cancellationToken);
    }
}
