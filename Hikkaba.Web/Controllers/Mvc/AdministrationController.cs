using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Shared.Constants;
using Hikkaba.Data.Entities;
using Hikkaba.Application.Contracts;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("Administration")]
internal sealed class AdministrationController : Controller
{
    private readonly IAdministrationService _administrationService;
    private readonly IBoardService _boardService;
    private readonly ISystemInfoService _systemInfoService;

    public AdministrationController(
        IAdministrationService administrationService,
        IBoardService boardService,
        ISystemInfoService systemInfoService)
    {
        _administrationService = administrationService;
        _boardService = boardService;
        _systemInfoService = systemInfoService;
    }

    [HttpGet]
    [Route("Administration")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var board = await _boardService.GetBoardAsync(cancellationToken);
        var dashboard = await _administrationService.GetDashboardAsync(cancellationToken);
        var systemInfo = _systemInfoService.GetSystemInfo();

        var dashboardViewModel = new DashboardViewModel
        {
            Board = board.ToViewModel(),
            CategoriesModerators = dashboard.CategoriesModerators.ToViewModels(),
            SystemInfo = systemInfo.ToViewModel(),
        };

        return View(dashboardViewModel);
    }
}
