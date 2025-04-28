using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("admin")]
public sealed class AdminController : BaseMvcController
{
    private readonly ISystemInfoService _systemInfoService;

    public AdminController(
        ISystemInfoService systemInfoService)
    {
        _systemInfoService = systemInfoService;
    }

    [HttpGet("dashboard", Name = "AdminDashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var systemInfo = _systemInfoService.GetSystemInfo();

        var dashboardViewModel = new DashboardViewModel
        {
            SystemInfo = systemInfo.ToViewModel(),
        };

        return View(dashboardViewModel);
    }
}
