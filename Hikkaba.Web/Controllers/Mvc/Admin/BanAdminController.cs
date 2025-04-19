using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Ban;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.BansViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        IBanService banService,
        IBanCreationPrerequisiteService banCreationPrerequisiteService,
        TimeProvider timeProvider)
    {
        _banService = banService;
        _banCreationPrerequisiteService = banCreationPrerequisiteService;
        _timeProvider = timeProvider;
    }

    [HttpGet("{id:int}", Name = "BanDetails")]
    public async Task<IActionResult> Details(
        [FromRoute] [Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        var ban = await _banService.GetBanAsync(id, cancellationToken);
        if (ban is null)
        {
            var returnUrl = GetLocalReferrerOrRoute("BanIndex");
            return CustomErrorPage(
                StatusCodes.Status404NotFound,
                "The requested ban was not found.",
                returnUrl);
        }

        return View(ban.ToViewModel());
    }

    [HttpGet("", Name = "BanIndex")]
    public async Task<IActionResult> Index(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int size = 10,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        var filter = new BanPagingFilter
        {
            EndsNotBefore = _timeProvider.GetUtcNow().UtcDateTime,
            PageNumber = page,
            PageSize = size,
            OrderBy =
            [
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
        [FromQuery] [Range(1, long.MaxValue)] long threadId,
        [FromQuery] [Range(1, long.MaxValue)] long postId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        var prerequisites = await _banCreationPrerequisiteService.GetPrerequisitesAsync(postId, threadId, cancellationToken);

        switch (prerequisites.Status)
        {
            case BanCreationPrerequisiteStatus.PostNotFound:
            {
                return CustomErrorPage(
                    StatusCodes.Status404NotFound,
                    "The requested post was not found.",
                    GetLocalReferrerOrNull());
            }
            case BanCreationPrerequisiteStatus.IpAddressIsLocalOrPrivate:
            {
                return CustomErrorPage(
                    StatusCodes.Status400BadRequest,
                    "The IP address is local or private.",
                    GetLocalReferrerOrNull());
            }
            case BanCreationPrerequisiteStatus.IpAddressNull:
            {
                return CustomErrorPage(
                    StatusCodes.Status400BadRequest,
                    "Can't retrieve user IP address.",
                    GetLocalReferrerOrNull());
            }
            case BanCreationPrerequisiteStatus.ActiveBanFound:
            {
                if (prerequisites.ActiveBanId.HasValue)
                {
                    return RedirectToRoute("BanDetails", new { id = prerequisites.ActiveBanId.Value });
                }

                return CustomErrorPage(
                    StatusCodes.Status400BadRequest,
                    "An active ban was found, but the ID is missing.",
                    GetLocalReferrerOrNull());
            }
            case BanCreationPrerequisiteStatus.Success:
            {
                break;
            }
            default:
            {
                return CustomErrorPage(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while retrieving ban creation prerequisites.",
                    GetLocalReferrerOrNull());
            }
        }

        if (prerequisites.Post == null)
        {
            return CustomErrorPage(
                StatusCodes.Status500InternalServerError,
                "Failed to retrieve necessary post information.",
                GetLocalReferrerOrNull());
        }

        if (prerequisites.IpAddressInfo == null)
        {
            return CustomErrorPage(
                StatusCodes.Status500InternalServerError,
                "Can't retrieve user IP address information.",
                GetLocalReferrerOrNull());
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
    public async Task<IActionResult> CreateConfirm(
        [Required] BanCreateViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();

            var prerequisites = await _banCreationPrerequisiteService.GetPrerequisitesAsync(viewModel.RelatedPostId, viewModel.RelatedThreadId, cancellationToken);

            switch (prerequisites.Status)
            {
                case BanCreationPrerequisiteStatus.PostNotFound:
                {
                    return CustomErrorPage(
                        StatusCodes.Status404NotFound,
                        "The requested post was not found.",
                        GetLocalReferrerOrNull());
                }
                case BanCreationPrerequisiteStatus.IpAddressIsLocalOrPrivate:
                {
                    return CustomErrorPage(
                        StatusCodes.Status400BadRequest,
                        "The IP address is local or private.",
                        GetLocalReferrerOrNull());
                }
                case BanCreationPrerequisiteStatus.IpAddressNull:
                {
                    return CustomErrorPage(
                        StatusCodes.Status400BadRequest,
                        "Can't retrieve user IP address.",
                        GetLocalReferrerOrNull());
                }
                case BanCreationPrerequisiteStatus.ActiveBanFound:
                {
                    if (prerequisites.ActiveBanId.HasValue)
                    {
                        return RedirectToRoute("BanDetails", new { id = prerequisites.ActiveBanId.Value });
                    }

                    return CustomErrorPage(
                        StatusCodes.Status400BadRequest,
                        "An active ban was found, but the ID is missing.",
                        GetLocalReferrerOrNull());
                }
                case BanCreationPrerequisiteStatus.Success:
                {
                    break;
                }
                default:
                {
                    return CustomErrorPage(
                        StatusCodes.Status500InternalServerError,
                        "An unexpected error occurred while retrieving ban creation prerequisites.",
                        GetLocalReferrerOrNull());
                }
            }

            if (prerequisites.Post == null)
            {
                return CustomErrorPage(
                    StatusCodes.Status500InternalServerError,
                    "Failed to retrieve necessary post information.",
                    GetLocalReferrerOrNull());
            }

            if (prerequisites.IpAddressInfo == null)
            {
                return CustomErrorPage(
                    StatusCodes.Status500InternalServerError,
                    "Can't retrieve user IP address information.",
                    GetLocalReferrerOrNull());
            }

            var postVm = prerequisites.Post.ToViewModel();
            var ipAddressVm = prerequisites.IpAddressInfo.ToViewModel();

            var vm = new BanCreateDataViewModel
            {
                PostDetails = postVm,
                IpAddressDetails = ipAddressVm,
            };

            return View("Create", vm);
        }

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
        var resultModel = await _banService.CreateBanAsync(command, cancellationToken);

        var result = resultModel.Match(
            success => RedirectToRoute("BanDetails", new { id = success.BanId }),
            err => CustomErrorPage(err.StatusCode, err.ErrorMessage, GetLocalReferrerOrNull()));

        return result;
    }

    [HttpGet("{id:int}/delete", Name = "BanDelete")]
    public async Task<IActionResult> Delete(
        [FromRoute] [Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        var ban = await _banService.GetBanAsync(id, cancellationToken);
        var viewModel = ban?.ToViewModel();
        return View(viewModel);
    }

    [HttpPost("{id:int}/delete", Name = "BanDeleteConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirm(
        [FromRoute] [Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        await _banService.SetBanDeletedAsync(id, true, cancellationToken);
        return RedirectToRoute("BanIndex");
    }
}
