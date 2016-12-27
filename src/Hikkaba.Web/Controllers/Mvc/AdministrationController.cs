using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Service;
using Hikkaba.Web.Filters;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Hikkaba.Web.ViewModels.BoardViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc
{
    // todo: add ban functions: 1) ban by ip 2) ban by ip range 3) ban and delete all posts in category 4) ban and delete all posts

    [TypeFilter(typeof(ExceptionLoggingFilter))]
    [Authorize(Roles = Defaults.DefaultAdminRoleName)]
    public class AdministrationController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAdministrationService _administrationService;
        private readonly IBoardService _boardService;
        private readonly ICategoryToModeratorService _categoryToModeratorService;

        public AdministrationController(IMapper mapper,
            IAdministrationService administrationService,
            IBoardService boardService,
            ICategoryToModeratorService categoryToModeratorService)
        {
            _mapper = mapper;
            _administrationService = administrationService;
            _boardService = boardService;
            _categoryToModeratorService = categoryToModeratorService;
        }

        public async Task<IActionResult> Index()
        {
            var boardDto = (await _boardService.ListAsync()).FirstOrDefault();
            var boardViewModel = _mapper.Map<BoardViewModel>(boardDto);
            var categoriesModerators = _categoryToModeratorService.ListCategoriesModerators();
            var dashboardViewModel = new DashboardViewModel();
            return View(dashboardViewModel);
        }

        public IActionResult DeleteAllContent()
        {
            return View();
        }

        [HttpPost, ActionName("DeleteAllContent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllContentConfirmed(Guid categoryId)
        {
            await _administrationService.DeleteAllContentAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}