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
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc
{
    [TypeFilter(typeof(ExceptionLoggingFilter))]
    [Authorize(Roles = Defaults.AdministratorRoleName)]
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

        [Route("Administration")]
        public async Task<IActionResult> Index()
        {
            var boardDto = (await _boardService.ListAsync()).FirstOrDefault();
            var boardViewModel = _mapper.Map<BoardViewModel>(boardDto);
            var categoriesModeratorsDtoList = await _categoryToModeratorService.ListCategoriesModeratorsAsync();
            var categoriesModeratorsViewModelList = new List<CategoryModeratorsViewModel>();
            foreach (var dtoPair in categoriesModeratorsDtoList)
            {
                categoriesModeratorsViewModelList.Add(new CategoryModeratorsViewModel()
                {
                    Category = _mapper.Map<CategoryViewModel>(dtoPair.Key),
                    Moderators = _mapper.Map<List<ApplicationUserViewModel>>(dtoPair.Value),
                });
            }
            var dashboardViewModel = new DashboardViewModel()
            {
               Board = boardViewModel,
               CategoriesModerators = categoriesModeratorsViewModelList,
            };
            return View(dashboardViewModel);
        }

        //public async 

        public IActionResult DeleteAllContent()
        {
            return View();
        }

        [HttpPost, ActionName("DeleteAllContent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllContentConfirmed()
        {
            await _administrationService.DeleteAllContentAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}