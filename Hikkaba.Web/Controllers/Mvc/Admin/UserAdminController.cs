using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("admin/users")]
public class UserAdminController : Controller
{
    [HttpGet("", Name = "UserIndex")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View();
    }
}
