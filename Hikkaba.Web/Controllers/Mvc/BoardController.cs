using System.Threading.Tasks;
using Hikkaba.Services.Contracts;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.BoardViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc;

public class BoardController : Controller
{
    private readonly ILogger<BoardController> _logger;
    private readonly IBoardService _boardService;

    public BoardController(
        ILogger<BoardController> logger,
        IBoardService boardService)
    {
        _logger = logger;
        _boardService = boardService;
    }

    [Route("Board/Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var board = await _boardService.GetBoardAsync();
        var viewModel = board.ToViewModel();
        return View(viewModel);
    }

    /*
    [Route("Board/Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BoardViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var dto = await _boardService.GetBoardAsync();
            _mapper.Map(viewModel, dto);
            await _boardService.EditBoardAsync(dto);
            return RedirectToAction("Index", "Administration");
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }
    */
}
