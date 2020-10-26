using TPrimaryKey = System.Guid;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Services;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.BoardViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc
{
    public class BoardController : Controller
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly IMapper _mapper;
        private readonly IBoardService _boardService;

        public BoardController(ILogger<CategoriesController> logger,
            IMapper mapper,
            IBoardService boardService)
        {
            _logger = logger;
            _mapper = mapper;
            _boardService = boardService;
        }
        
        [Route("Board/Edit")]
        public async Task<IActionResult> Edit(TPrimaryKey id)
        {
            var dto = await _boardService.GetBoardAsync();
            var viewModel = _mapper.Map<BoardViewModel>(dto);
            return View(viewModel);
        }

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
    }
}