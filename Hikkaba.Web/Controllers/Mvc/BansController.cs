using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Shared.Constants;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Models;
using Hikkaba.Application.Contracts;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.BansViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("Bans")]
internal sealed class BansController : BaseMvcController
{
    private readonly IBanService _banService;
    private readonly IBanCreationPrerequisiteService _banCreationPrerequisiteService;
    private readonly TimeProvider _timeProvider;

    public BansController(
        UserManager<ApplicationUser> userManager,
        IBanService banService,
        IBanCreationPrerequisiteService banCreationPrerequisiteService,
        TimeProvider timeProvider)
        : base(userManager)
    {
        _banService = banService;
        _banCreationPrerequisiteService = banCreationPrerequisiteService;
        _timeProvider = timeProvider;
    }

    [HttpGet]
    [Route("Bans/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var ban = await _banService.GetBanAsync(id, cancellationToken);
        if (ban is null)
        {
            return NotFound("Ban not found");
        }

        return View(ban.ToViewModel());
    }

    [HttpGet]
    [Route("Bans")]
    public async Task<IActionResult> Index(
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var filter = new BanPagingFilter
        {
            EndsNotBefore = _timeProvider.GetUtcNow().UtcDateTime,
        };
        var bans = await _banService.ListBansPaginatedAsync(filter, cancellationToken);

        var viewModelList = new BanIndexViewModel
        {
            Bans = new PagedResult<BanViewModel>(bans.Data.ToViewModels(), filter, bans.TotalItemCount),
        };
        return View(viewModelList);
    }

    [HttpGet]
    [Route("Bans/Create")]
    public async Task<IActionResult> Create(string categoryAlias, long threadId, long postId, CancellationToken cancellationToken)
    {
        var prerequisites = await _banCreationPrerequisiteService.GetPrerequisitesAsync(postId, threadId, cancellationToken);

        switch (prerequisites.Status)
        {
            case BanCreationPrerequisiteStatus.PostNotFound:
                return NotFound("Post not found");
            case BanCreationPrerequisiteStatus.IpAddressNull:
                return Problem("User IP address is null");
            case BanCreationPrerequisiteStatus.ActiveBanFound:
                if (prerequisites.ActiveBanId.HasValue)
                {
                    return RedirectToAction("Details", new { id = prerequisites.ActiveBanId.Value });
                }

                return Problem("Active ban found but ID is missing.");
            case BanCreationPrerequisiteStatus.Success:
                break;
            default:
                return Problem("An unexpected error occurred while retrieving ban creation prerequisites.");
        }

        if (prerequisites.Post == null || prerequisites.IpAddressInfo == null)
        {
            return Problem("Failed to retrieve necessary post or IP address information.");
        }

        var postVm = prerequisites.Post.ToViewModel();
        var ipAddressVm = prerequisites.IpAddressInfo.ToViewModel();

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

            return RedirectToAction("Details", new { id = id });
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();

            var prerequisites = await _banCreationPrerequisiteService.GetPrerequisitesAsync(viewModel.RelatedPostId, viewModel.RelatedThreadId, cancellationToken);

            switch (prerequisites.Status)
            {
                case BanCreationPrerequisiteStatus.PostNotFound:
                    return NotFound("Post not found");
                case BanCreationPrerequisiteStatus.IpAddressNull:
                    return Problem("User IP address is null");
                case BanCreationPrerequisiteStatus.ActiveBanFound:
                    if (prerequisites.ActiveBanId.HasValue)
                    {
                        return RedirectToAction("Details", new { id = prerequisites.ActiveBanId.Value });
                    }

                    return Problem("Active ban found but ID is missing.");
                case BanCreationPrerequisiteStatus.Success:
                    break;
                default:
                    return Problem("An unexpected error occurred while retrieving ban creation prerequisites.");
            }

            if (prerequisites.Post == null || prerequisites.IpAddressInfo == null)
            {
                return Problem("Failed to retrieve necessary post or IP address information.");
            }

            var postVm = prerequisites.Post.ToViewModel();
            var ipAddressVm = prerequisites.IpAddressInfo.ToViewModel();

            var vm = new BanCreateDataViewModel
            {
                PostDetails = postVm,
                IpAddressDetails = ipAddressVm,
            };

            return View(vm);
        }
    }

    [HttpGet]
    [Route("Bans/{id:int}/Delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ban = await _banService.GetBanAsync(id, cancellationToken);
        var viewModel = ban?.ToViewModel();
        return View(viewModel);
    }

    [Route("Bans/{id:int}/Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _banService.SetBanDeletedAsync(id, true, cancellationToken);
        return RedirectToAction("Index");
    }
}
