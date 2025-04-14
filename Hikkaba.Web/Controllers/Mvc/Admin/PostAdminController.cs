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
public sealed class PostAdminController : BaseMvcController
{
    private readonly IPostService _postService;

    public PostAdminController(
        IPostService postService)
    {
        _postService = postService;
    }

    /*
     todo: in order to edit posts, we need to store the original post markup in the database

    [HttpGet("{postId:long}/edit", Name = "PostEdit")]
    public async Task<IActionResult> Edit(
        [Required] [FromRoute] long postId,
        CancellationToken cancellationToken)
    {
        var post = await _postService.GetPostAsync(postId, cancellationToken);
        if (post is null)
        {
            return CustomErrorPage(
                StatusCodes.Status404NotFound,
                "The requested post was not found.",
                GetLocalReferrerOrRoute("HomeIndex"));
        }

        var viewModel = post.ToEditViewModel();

        return View(viewModel);
    }

    [HttpPost("{postId:long}/edit", Name = "PostEditConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirm(
        [Required] [FromRoute] long postId,
        [Required] [FromForm] PostEditViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View("Edit", viewModel);
        }

        var requestModel = viewModel.ToEditModel();
        await _postService.EditPostAsync(requestModel, cancellationToken);

        return RedirectToRoute(
            "ThreadDetails",
            new { categoryAlias = viewModel.CategoryAlias, threadId = viewModel.ThreadId },
            postId.ToString(CultureInfo.InvariantCulture));
    }
    */

    [HttpPost("{postId:long}", Name = "PostSetDeleted")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDeleted(
        [Required] [FromRoute] [Range(1, long.MaxValue)] long postId,
        [Required] [FromForm] bool isDeleted,
        [Required] [FromForm] [MaxLength(Defaults.MaxCategoryAliasLength)] string categoryAlias,
        [Required] [FromForm] [Range(1, long.MaxValue)] long threadId,
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
