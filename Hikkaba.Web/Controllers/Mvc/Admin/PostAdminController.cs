using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName + "," + Defaults.ModeratorRoleName)]
[Route("admin/posts")]
public sealed class PostAdminController : BaseMvcController
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
        [Required] [FromRoute] [Range(1, long.MaxValue)] long postId,
        [Required] [FromForm] bool isDeleted,
        [Required] [FromForm] [MaxLength(Defaults.MaxCategoryAliasLength)] string categoryAlias,
        [Required] [FromForm] [Range(1, long.MaxValue)] long threadId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

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
