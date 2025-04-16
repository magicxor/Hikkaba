using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPost("{threadId:long}/set-pinned", Name = "ThreadSetPinned")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPinned(
        [Required] [FromRoute] [Range(1, long.MaxValue)] long threadId,
        [Required] [FromForm] bool isPinned,
        [Required] [FromForm] [MaxLength(Defaults.MaxCategoryAliasLength)] string categoryAlias,
        CancellationToken cancellationToken)
    {
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
