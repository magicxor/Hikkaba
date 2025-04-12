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
[Route("admin/posts")]
public class PostAdminController : BaseMvcController
{
    private readonly IPostService _postService;

    public PostAdminController(
        IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost("{postId:long}", Name = "PostSetDeleted")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDeleted(
        [Required] [FromRoute] long postId,
        [Required] [FromForm] bool isDeleted,
        [Required] [FromForm] string categoryAlias,
        [Required] [FromForm] long threadId,
        CancellationToken cancellationToken)
    {
        await _postService.SetPostDeletedAsync(postId, isDeleted, cancellationToken);

        var redirectUrl = GetLocalReferrerOrRoute(
            "ThreadDetails",
            new
            {
                categoryAlias = categoryAlias,
                threadId = threadId,
            },
            postId.ToString(CultureInfo.InvariantCulture)) ?? "/";

        return LocalRedirect(redirectUrl);
    }
}
