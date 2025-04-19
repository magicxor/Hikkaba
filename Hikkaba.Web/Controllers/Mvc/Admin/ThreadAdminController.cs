using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName + "," + Defaults.ModeratorRoleName)]
[Route("admin/threads")]
public sealed class ThreadAdminController : BaseMvcController
{
    private readonly IThreadService _threadService;

    public ThreadAdminController(
        IThreadService threadService)
    {
        _threadService = threadService;
    }

    [HttpGet("{threadId:long}", Name = "ThreadEdit")]
    public async Task<IActionResult> Edit(
        [Required] [FromRoute] [Range(1, long.MaxValue)]
        long threadId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        var thread = await _threadService.GetThreadDetailsAsync(threadId, cancellationToken);
        if (thread is null)
        {
            var returnUrl = GetLocalReferrerOrNull();
            return CustomErrorPage(
                StatusCodes.Status404NotFound,
                "The requested thread was not found.",
                returnUrl);
        }

        var viewModel = thread.ToEditViewModel();
        return View(viewModel);
    }

    [HttpPost("{threadId:long}", Name = "ThreadEditConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirm(
        [Required] [FromRoute] [Range(1, long.MaxValue)]
        long threadId,
        [Required] [FromForm]
        ThreadEditViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View("Edit", viewModel);
        }

        var editRequestModel = viewModel.ToEditRequestModel();
        var result = await _threadService.EditThreadAsync(editRequestModel, cancellationToken);

        return result.Match(
            _ =>
            {
                var returnUrl = GetLocalReferrerOrRoute(
                    "ThreadDetails",
                    new
                    {
                        categoryAlias = viewModel.CategoryAlias,
                        threadId = threadId,
                    }) ?? "/";
                return LocalRedirect(returnUrl);
            },
            err =>
            {
                var returnUrl = GetLocalReferrerOrRoute(
                    "ThreadDetails",
                    new
                    {
                        categoryAlias = viewModel.CategoryAlias,
                        threadId = threadId,
                    });
                return CustomErrorPage(
                    err.StatusCode,
                    err.ErrorMessage,
                    returnUrl);
            });
    }

    [HttpPost("{threadId:long}/set-pinned", Name = "ThreadSetPinned")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPinned(
        [Required] [FromRoute] [Range(1, long.MaxValue)] long threadId,
        [Required] [FromForm] bool isPinned,
        [Required] [FromForm] [MaxLength(Defaults.MaxCategoryAliasLength)] string categoryAlias,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        await _threadService.SetThreadPinnedAsync(threadId, isPinned, cancellationToken);

        var redirectUrl = GetLocalReferrerOrRoute(
            "ThreadDetails",
            new
            {
                categoryAlias = categoryAlias,
                threadId = threadId,
            }) ?? "/";

        return LocalRedirect(redirectUrl);
    }

    [HttpPost("{threadId:long}/set-cyclic", Name = "ThreadSetCyclic")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCyclic(
        [Required] [FromRoute] [Range(1, long.MaxValue)] long threadId,
        [Required] [FromForm] bool isCyclic,
        [Required] [FromForm] [MaxLength(Defaults.MaxCategoryAliasLength)] string categoryAlias,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        await _threadService.SetThreadCyclicAsync(threadId, isCyclic, cancellationToken);

        var redirectUrl = GetLocalReferrerOrRoute(
            "ThreadDetails",
            new
            {
                categoryAlias = categoryAlias,
                threadId = threadId,
            }) ?? "/";

        return LocalRedirect(redirectUrl);
    }

    [HttpPost("{threadId:long}/set-closed", Name = "ThreadSetClosed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetClosed(
        [Required] [FromRoute] [Range(1, long.MaxValue)] long threadId,
        [Required] [FromForm] bool isClosed,
        [Required] [FromForm] [MaxLength(Defaults.MaxCategoryAliasLength)] string categoryAlias,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        await _threadService.SetThreadClosedAsync(threadId, isClosed, cancellationToken);

        var redirectUrl = GetLocalReferrerOrRoute(
            "ThreadDetails",
            new
            {
                categoryAlias = categoryAlias,
                threadId = threadId,
            }) ?? "/";

        return LocalRedirect(redirectUrl);
    }

    [HttpPost("{threadId:long}/set-deleted", Name = "ThreadSetDeleted")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDeleted(
        [Required] [FromRoute] [Range(1, long.MaxValue)] long threadId,
        [Required] [FromForm] bool isDeleted,
        [Required] [FromForm] [MaxLength(Defaults.MaxCategoryAliasLength)] string categoryAlias,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        await _threadService.SetThreadDeletedAsync(threadId, isDeleted, cancellationToken);

        var redirectUrl = GetLocalReferrerOrRoute(
            "ThreadDetails",
            new
            {
                categoryAlias = categoryAlias,
                threadId = threadId,
            }) ?? "/";

        return LocalRedirect(redirectUrl);
    }
}
