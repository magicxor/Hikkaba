using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.ViewModels.BoardViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("admin/board")]
public sealed class BoardAdminController : BaseMvcController
{
    private readonly IBoardService _boardService;

    public BoardAdminController(IBoardService boardService)
    {
        _boardService = boardService;
    }

    [HttpGet("edit", Name = "BoardEdit")]
    public async Task<IActionResult> Edit(
        CancellationToken cancellationToken)
    {
        var board = await _boardService.GetBoardAsync(cancellationToken);
        var vm = board.ToViewModel();
        return View(vm);
    }

    [HttpPost("edit", Name = "BoardEditConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirm(
        BoardViewModel boardViewModel,
        CancellationToken cancellationToken)
    {
        await _boardService.EditBoardAsync(boardViewModel.Name, cancellationToken);
        return RedirectToRoute("AdminDashboard");
    }
}
