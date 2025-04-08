using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.BansViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName + "," + Defaults.ModeratorRoleName)]
[Route("admin/bans")]
public sealed class BanAdminController : BaseMvcController
{
    private readonly IBanService _banService;
    private readonly IBanCreationPrerequisiteService _banCreationPrerequisiteService;
    private readonly TimeProvider _timeProvider;

    public BanAdminController(
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

    [HttpGet("{id:int}", Name = "BanDetails")]
    public async Task<IActionResult> Details(
        [FromRoute][Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var ban = await _banService.GetBanAsync(id, cancellationToken);
        if (ban is null)
        {
            return NotFound("Ban not found");
        }

        return View(ban.ToViewModel());
    }

    [HttpGet("", Name = "BanIndex")]
    public async Task<IActionResult> Index(
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var filter = new BanPagingFilter
        {
            EndsNotBefore = _timeProvider.GetUtcNow().UtcDateTime,
            PageNumber = page,
            PageSize = size,
            OrderBy = [
                new OrderByItem { Field = nameof(BanDetailsModel.CreatedAt), Direction = OrderByDirection.Desc },
                new OrderByItem { Field = nameof(BanDetailsModel.Id), Direction = OrderByDirection.Desc },
            ],
        };
        var bans = await _banService.ListBansPaginatedAsync(filter, cancellationToken);

        var viewModelList = new BanIndexViewModel
        {
            Bans = new PagedResult<BanViewModel>(bans.Data.ToViewModels(), filter, bans.TotalItemCount),
        };
        return View(viewModelList);
    }

    [HttpGet("create", Name = "BanCreate")]
    public async Task<IActionResult> Create(
        [FromQuery][Range(1, long.MaxValue)] long threadId,
        [FromQuery][Range(1, long.MaxValue)] long postId,
        CancellationToken cancellationToken)
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

    [HttpPost("create", Name = "BanCreateConfirm")]
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

    [HttpGet("{id:int}/delete", Name = "BanDelete")]
    public async Task<IActionResult> Delete(
        [FromRoute][Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var ban = await _banService.GetBanAsync(id, cancellationToken);
        var viewModel = ban?.ToViewModel();
        return View(viewModel);
    }

    [HttpPost("{id:int}/delete", Name = "BanDeleteConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        [FromRoute][Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        await _banService.SetBanDeletedAsync(id, true, cancellationToken);
        return RedirectToAction("Index");
    }
}
