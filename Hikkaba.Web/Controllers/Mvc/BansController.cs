using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Shared.Constants;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Models;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Shared.Enums;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Utils;
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
    private readonly IPostService _postService;
    private readonly IGeoIpService _geoIpService;

    public BansController(
        UserManager<ApplicationUser> userManager,
        IBanService banService,
        IPostService postService,
        IGeoIpService geoIpService) : base(userManager)
    {
        _banService = banService;
        _postService = postService;
        _geoIpService = geoIpService;
    }

    [Route("Bans/{id}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var ban = await _banService.GetBanAsync(id, cancellationToken);
        if (ban is null)
        {
            return NotFound("Ban not found");
        }

        return View(ban.ToViewModel());
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
    public async Task<IActionResult> Create(string categoryAlias, long threadId, long postId, CancellationToken cancellationToken)
    {
        var threadPosts = await _postService.ListThreadPostsAsync(new ThreadPostsFilter
        {
            PostId = postId,
            ThreadId = threadId,
            IncludeDeleted = true,
        }, cancellationToken);

        if (threadPosts.Count == 0)
        {
            return NotFound("Post not found");
        }

        var threadPost = threadPosts[0];

        if (threadPost.UserIpAddress == null)
        {
            return Problem("User IP address is null");
        }

        var activeBan = await _banService.FindActiveBan(new ActiveBanFilter
        {
            UserIpAddress = threadPost.UserIpAddress,
            CategoryAlias = threadPost.CategoryAlias,
            ThreadId = threadPost.ThreadId,
        }, cancellationToken);

        if (activeBan != null)
        {
            return RedirectToAction("Details", new {id = activeBan.Id});
        }

        var postVm = threadPost.ToViewModel();
        var ipAddressVm = _geoIpService.GetIpAddressInfo(new IPAddress(threadPost.UserIpAddress)).ToViewModel();

        var vm = new BanCreateDataViewModel
        {
            PostDetails = postVm,
            IpAddressDetails = ipAddressVm,
        };

        return View(vm);
    }

    [Route("Bans/Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BanCreateViewModel viewModel, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var command = new BanCreateCommand
            {
                EndsAt = viewModel.EndsAt,
                BannedIpAddress = viewModel.BannedIpAddress,
                BanByNetwork = viewModel.BanByNetwork,
                BanInAllCategories = viewModel.BanInAllCategories,
                AdditionalAction = viewModel.AdditionalAction,
                Reason = viewModel.Reason,
                RelatedPostId = viewModel.RelatedPostId,
                RelatedThreadId = viewModel.RelatedThreadId,
                CategoryAlias = viewModel.CategoryAlias,
            };
            var id = await _banService.CreateBanAsync(command, cancellationToken);

            return RedirectToAction("Details", new {id = id});
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();

            var threadPosts = await _postService.ListThreadPostsAsync(new ThreadPostsFilter
            {
                PostId = viewModel.RelatedPostId,
                ThreadId = viewModel.RelatedThreadId,
                IncludeDeleted = true,
            }, cancellationToken);

            if (threadPosts.Count == 0)
            {
                return NotFound("Post not found");
            }

            var threadPost = threadPosts[0];

            if (threadPost.UserIpAddress == null)
            {
                return Problem("User IP address is null");
            }

            var activeBan = await _banService.FindActiveBan(new ActiveBanFilter
            {
                UserIpAddress = threadPost.UserIpAddress,
                CategoryAlias = threadPost.CategoryAlias,
                ThreadId = threadPost.ThreadId,
            }, cancellationToken);

            if (activeBan != null)
            {
                return RedirectToAction("Details", new {id = activeBan.Id});
            }

            var postVm = threadPost.ToViewModel();
            var ipAddressVm = _geoIpService.GetIpAddressInfo(new IPAddress(threadPost.UserIpAddress)).ToViewModel();

            var vm = new BanCreateDataViewModel
            {
                PostDetails = postVm,
                IpAddressDetails = ipAddressVm,
            };

            return View(vm);
        }
    }

    /*
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
