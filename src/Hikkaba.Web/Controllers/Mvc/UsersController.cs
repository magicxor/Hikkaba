using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Dto;
using Hikkaba.Service;
using Hikkaba.Web.Filters;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [Route("Users/{userId}")]
        public async Task<IActionResult> Details(Guid userId)
        {
            var dto = _applicationUserService.GetAsync(userId);
            var viewModel = _mapper.Map<ApplicationUserViewModel>(dto);
            return View(viewModel);
        }

        [Route("Users")]
        public async Task<IActionResult> Index()
        {
            var dtoList = await _applicationUserService.ListAsync();
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
            var dto = _mapper.Map<ApplicationUserDto>(viewModel);
            var id = await _applicationUserService.CreateAsync(dto);
            return RedirectToAction("Index");
        }

        [Route("Users/{userId}/Edit")]
        public async Task<IActionResult> Edit(Guid userId)
        {
            var dto = await _applicationUserService.GetAsync(userId);
            var viewModel = _mapper.Map<ApplicationUserViewModel>(dto);
            return View(viewModel);
        }

        [Route("Users/{userId}/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUserViewModel viewModel)
        {
            var dto = _mapper.Map<ApplicationUserDto>(viewModel);
            await _applicationUserService.EditAsync(dto);
            return RedirectToAction("Index");
        }

        [Route("Users/{userId}/Delete")]
        public async Task<IActionResult> Delete(Guid userId)
        {
            var dto = await _applicationUserService.GetAsync(userId);
            var viewModel = _mapper.Map<ApplicationUserViewModel>(dto);
            return View(viewModel);
        }

        [Route("Users/{userId}/Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid userId)
        {
            await _applicationUserService.DeleteAsync(userId);
            return RedirectToAction("Index");
        }
    }
}