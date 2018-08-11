using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Models.Dto;
using Hikkaba.Services;
using Hikkaba.Web.Filters;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Web.Controllers.Mvc
{
    // todo: implement views

    [TypeFilter(typeof(ExceptionLoggingFilter))]
    [Authorize(Roles = Defaults.AdministratorRoleName)]
    public class UsersController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IApplicationUserService _applicationUserService;

        public UsersController(IMapper mapper, IApplicationUserService applicationUserService)
        {
            _mapper = mapper;
            _applicationUserService = applicationUserService;
        }

        [Route("Users/{id}")]
        public async Task<IActionResult> Details(TPrimaryKey id)
        {
            var dto = await _applicationUserService.GetAsync(id);
            var viewModel = _mapper.Map<ApplicationUserViewModel>(dto);
            return View(viewModel);
        }

        [Route("Users")]
        public async Task<IActionResult> Index()
        {
            var dtoList = await _applicationUserService.ListAsync(user => true, user => user.UserName);
            var viewModelList = _mapper.Map<List<ApplicationUserViewModel>>(dtoList);
            return View(viewModelList);
        }

        [Route("Users/Create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [Route("Users/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var dto = _mapper.Map<ApplicationUserDto>(viewModel);
                var id = await _applicationUserService.CreateAsync(dto);
                return RedirectToAction("Details", new { id = id });
            }
            else
            {
                ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
                return View(viewModel);
            }
        }

        [Route("Users/{id}/Edit")]
        public async Task<IActionResult> Edit(TPrimaryKey id)
        {
            var dto = await _applicationUserService.GetAsync(id);
            var viewModel = _mapper.Map<ApplicationUserViewModel>(dto);
            return View(viewModel);
        }

        [Route("Users/{id}/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var dto = _mapper.Map<ApplicationUserDto>(viewModel);
                await _applicationUserService.EditAsync(dto);
                return RedirectToAction("Details", new { id = dto.Id });
            }
            else
            {
                ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
                return View(viewModel);
            }
        }

        [Route("Users/{id}/Delete")]
        public async Task<IActionResult> Delete(TPrimaryKey id)
        {
            var dto = await _applicationUserService.GetAsync(id);
            var viewModel = _mapper.Map<ApplicationUserViewModel>(dto);
            return View(viewModel);
        }

        [Route("Users/{id}/Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(TPrimaryKey id)
        {
            await _applicationUserService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}