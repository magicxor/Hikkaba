using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Service;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Filters;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Hikkaba.Web.ViewModels.BansViewModels;
using Hikkaba.Web.ViewModels.BoardViewModels;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc
{
    // todo: add ban for specific category (select category from list)
    // todo: add ability to attach related post to ban
    // todo: add datetimepicker which will convert time to UTC
    // todo: add ban functions: 1) ban by ip 2) ban by ip range 3) ban and delete all posts in category 4) ban and delete all posts

    [TypeFilter(typeof(ExceptionLoggingFilter))]
    [Authorize(Roles = Defaults.AdministratorRoleName)]
    public class BansController : BaseMvcController
    {
        private readonly IMapper _mapper;
        private readonly IBanService _banService;
        
        public BansController(UserManager<ApplicationUser> userManager, 
            IMapper mapper, 
            IBanService banService) : base(userManager)
        {
            _mapper = mapper;
            _banService = banService;
        }

        [Route("Bans/{banId}")]
        public async Task<IActionResult> Details(Guid banId)
        {
            var banDto = await _banService.GetAsync(banId);
            var banViewModel = _mapper.Map<BanViewModel>(banDto);
            return View(banViewModel);
        }

        [Route("Bans")]
        public async Task<IActionResult> Index()
        {
            var dtoList = await _banService.ListAsync();
            var viewModelList = _mapper.Map<List<BanViewModel>>(dtoList);
            return View(viewModelList);
        }

        [Route("Bans/Create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [Route("Bans/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BanViewModel ban)
        {
            var dto = _mapper.Map<BanDto>(ban);
            var id = await _banService.CreateOrGetIdAsync(dto, CurrentUserId);
            // todo: notification about existing ban
            return RedirectToAction("Details", new { banId = id });
        }

        [Route("Bans/{banId}/Edit")]
        public async Task<IActionResult> Edit(Guid banId)
        {
            var dto = await _banService.GetAsync(banId);
            var viewModel = _mapper.Map<BanViewModel>(dto);
            return View(viewModel);
        }

        [Route("Bans/{banId}/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BanViewModel ban)
        {
            var dto = _mapper.Map<BanDto>(ban);
            await _banService.EditAsync(dto, CurrentUserId);
            return RedirectToAction("Details", new { banId = dto.Id });
        }

        [Route("Bans/{banId}/Delete")]
        public async Task<IActionResult> Delete(Guid banId)
        {
            var dto = await _banService.GetAsync(banId);
            var viewModel = _mapper.Map<BanViewModel>(dto);
            return View(viewModel);
        }

        [Route("Bans/{banId}/Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid banId)
        {
            await _banService.DeleteAsync(banId, CurrentUserId);
            return RedirectToAction("Index");
        }
    }
}