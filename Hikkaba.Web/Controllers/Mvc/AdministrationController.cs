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
public class AdministrationController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAdministrationService _administrationService;
    private readonly IBoardService _boardService;
    private readonly ISystemInfoService _systemInfoService;

    public AdministrationController(
        SignInManager<ApplicationUser> signInManager,
        IAdministrationService administrationService,
        IBoardService boardService,
        ISystemInfoService systemInfoService)
    {
        _signInManager = signInManager;
        _administrationService = administrationService;
        _boardService = boardService;
        _systemInfoService = systemInfoService;
    }

    [Route("Administration")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var board = await _boardService.GetBoardAsync();
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
