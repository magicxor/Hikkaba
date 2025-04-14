using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("admin/board")]
public sealed class BoardAdminController : BaseMvcController
{
    [HttpGet("edit", Name = "BoardEdit")]
    public async Task<IActionResult> Edit(
        CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpPost("edit", Name = "BoardEditConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirm(
        CancellationToken cancellationToken)
    {
        return View("Edit");
    }
}
