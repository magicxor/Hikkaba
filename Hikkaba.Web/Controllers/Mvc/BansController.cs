using System;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Shared.Constants;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Models;
using Hikkaba.Application.Contracts;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.ViewModels.BansViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc;
// todo: add ban for specific category (select category from list)
// todo: add ability to attach related post to ban
// todo: add ban functions: 1) ban by ip 2) ban by ip range 3) ban and delete all posts in category 4) ban and delete all posts

[Authorize(Roles = Defaults.AdministratorRoleName)]
public class BansController : BaseMvcController
{
    private readonly IBanService _banService;

    public BansController(
        UserManager<ApplicationUser> userManager,
        IBanService banService) : base(userManager)
    {
        _banService = banService;
    }

    [Route("Bans/{id}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var ban = await _banService.GetBanAsync(id, cancellationToken);
        var viewModel = ban?.ToViewModel();

        if (viewModel is null)
        {
            return View();
        }
        else
        {
            return View(viewModel);
        }
    }

    [Route("Bans")]
    public async Task<IActionResult> Index(int page = 1, int size = 10, CancellationToken cancellationToken = default)
    {
        var filter = new BanPagingFilter
        {
            EndsNotBefore = DateTime.UtcNow,
        };
        var bans = await _banService.ListBansPaginatedAsync(filter, cancellationToken);

        var viewModelList = new BanIndexViewModel
        {
            Bans = new PagedResult<BanViewModel>(bans.Data.ToViewModels(), filter, bans.TotalItemCount),
        };
        return View(viewModelList);
    }

    [Route("Bans/Create")]
    public IActionResult Create()
    {
        return View();
    }

    /*
    [Route("Bans/Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BanCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var dto = _mapper.Map<BanCreateRequestSm>(viewModel);
            var id = await _banService.CreateBanAsync(dto);

            return RedirectToAction("Details", new {id = id});
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }

    [Route("Bans/{id}/Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _banService.GetBanAsync(id);
        var viewModel = _mapper.Map<BanEditViewModel>(dto);
        return View(viewModel);
    }

    [Route("Bans/{id}/Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BanEditViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var dto = _mapper.Map<BanEditRequestViewModel>(viewModel);
            await _banService.EditAsync(dto);
            return RedirectToAction("Details", new {id = dto.Id});
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }
    */

    [Route("Bans/{id}/Delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ban = await _banService.GetBanAsync(id, cancellationToken);
        var viewModel = ban?.ToViewModel();
        return View(viewModel);
    }

    [Route("Bans/{id}/Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _banService.SetBanDeletedAsync(id, true, cancellationToken);
        return RedirectToAction("Index");
    }
}
