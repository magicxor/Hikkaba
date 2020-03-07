using TPrimaryKey = System.Guid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Models.Dto;
using Hikkaba.Data.Entities;
using Hikkaba.Services;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.ViewModels.BansViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hikkaba.Web.Utils;

namespace Hikkaba.Web.Controllers.Mvc
{
    // todo: add ban for specific category (select category from list)
    // todo: add ability to attach related post to ban
    // todo: add ban functions: 1) ban by ip 2) ban by ip range 3) ban and delete all posts in category 4) ban and delete all posts

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

        [Route("Bans/{id}")]
        public async Task<IActionResult> Details(TPrimaryKey id)
        {
            var dto = await _banService.GetAsync(id);
            var viewModel = _mapper.Map<BanViewModel>(dto);
            return View(viewModel);
        }

        [Route("Bans")]
        public async Task<IActionResult> Index()
        {
            // todo: pagination
            var dtoList = await _banService.ListAsync(ban => (ban.End >= DateTime.UtcNow), ban => ban.Created, true);
            var viewModelList = _mapper.Map<List<BanViewModel>>(dtoList);
            return View(viewModelList);
        }

        [Route("Bans/Create")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("Bans/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BanViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var dto = _mapper.Map<BanDto>(viewModel);
                var id = await _banService.GetOrCreateAsync(dto);
                
                return RedirectToAction("Details", new {id = id});
            }
            else
            {
                ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
                return View(viewModel);
            }
        }

        [Route("Bans/{id}/Edit")]
        public async Task<IActionResult> Edit(TPrimaryKey id)
        {
            var dto = await _banService.GetAsync(id);
            var viewModel = _mapper.Map<BanViewModel>(dto);
            return View(viewModel);
        }

        [Route("Bans/{id}/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BanViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var dto = _mapper.Map<BanDto>(viewModel);
                await _banService.EditAsync(dto);
                return RedirectToAction("Details", new {id = dto.Id});
            }
            else
            {
                ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
                return View(viewModel);
            }
        }

        [Route("Bans/{id}/Delete")]
        public async Task<IActionResult> Delete(TPrimaryKey id)
        {
            var dto = await _banService.GetAsync(id);
            var viewModel = _mapper.Map<BanViewModel>(dto);
            return View(viewModel);
        }

        [Route("Bans/{id}/Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(TPrimaryKey id)
        {
            await _banService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}