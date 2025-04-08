using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("admin/board")]
public class BoardAdminController : Controller
{
    private readonly ILogger<BoardAdminController> _logger;
    private readonly IBoardService _boardService;

    public BoardAdminController(
        ILogger<BoardAdminController> logger,
        IBoardService boardService)
    {
        _logger = logger;
        _boardService = boardService;
    }

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
        return View();
    }
}
