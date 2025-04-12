using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using DNTCaptcha.Core;
using Hikkaba.Shared.Enums;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hikkaba.Web.Utils;
using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Extensions;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Services.Contracts;
using Hikkaba.Web.ViewModels.SearchViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc;

[AllowAnonymous]
[Route("posts")]
public sealed class PostController : BaseMvcController
{
    private readonly ILogger<PostController> _logger;
    private readonly IMessagePostProcessor _messagePostProcessor;
    private readonly IThreadService _threadService;
    private readonly IPostService _postService;

    public PostController(
        ILogger<PostController> logger,
        IMessagePostProcessor messagePostProcessor,
        IThreadService threadService,
        IPostService postService)
    {
        _logger = logger;
        _messagePostProcessor = messagePostProcessor;
        _threadService = threadService;
        _postService = postService;
    }

    [HttpGet("/{categoryAlias}/{threadId:long}/create", Name = "PostCreate")]
    public async Task<IActionResult> Create(
        [Required] [FromRoute] [MinLength(1)] [MaxLength(Defaults.MaxCategoryAliasLength)]
        string categoryAlias,
        [Required] [FromRoute] [Range(1, long.MaxValue)]
        long threadId,
        CancellationToken cancellationToken)
    {
        var filter = new CategoryThreadFilter
        {
            CategoryAlias = categoryAlias,
            ThreadId = threadId,
            IncludeDeleted = false,
        };

        var thread = await _threadService.GetCategoryThreadAsync(filter, cancellationToken);

        if (thread is null)
        {
            var returnUrl = GetLocalReferrerOrRoute("CategoryDetails", new { categoryAlias });
            return CustomErrorPage(
                StatusCodes.Status404NotFound,
                "The requested thread was not found.",
                returnUrl);
        }

        var postAnonymousCreateViewModel = new PostAnonymousCreateViewModel
        {
            IsSageEnabled = false,
            Message = string.Empty,
            Attachments = new FormFileCollection(),
            CategoryAlias = thread.CategoryAlias,
            CategoryName = thread.CategoryName,
            ThreadId = thread.ThreadId,
        };
        return View(postAnonymousCreateViewModel);
    }

    [HttpPost("/{categoryAlias}/{threadId:long}/create", Name = "PostCreateConfirm")]
    [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirm(
        [Required] [FromRoute] [MinLength(1)] [MaxLength(Defaults.MaxCategoryAliasLength)]
        string categoryAlias,
        [Required] [FromRoute] [Range(1, long.MaxValue)]
        long threadId,
        PostAnonymousCreateViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var postCreateRm = new PostCreateRequestModel
                {
                    BlobContainerId = Guid.NewGuid(),
                    IsSageEnabled = viewModel.IsSageEnabled,
                    MessageHtml = _messagePostProcessor.MessageToSafeHtml(viewModel.CategoryAlias, viewModel.ThreadId, viewModel.Message),
                    MessageText = _messagePostProcessor.MessageToPlainText(viewModel.Message),
                    UserIpAddress = UserIpAddressBytes,
                    UserAgent = UserAgent.TryLeft(Defaults.MaxUserAgentLength),
                    CategoryAlias = viewModel.CategoryAlias,
                    ThreadId = viewModel.ThreadId,
                    MentionedPosts = _messagePostProcessor.GetMentionedPosts(viewModel.Message),
                };

                if (postCreateRm.MessageText.Length > Defaults.MaxMessageLength)
                {
                    ModelState.AddModelError(nameof(viewModel.Message), $"Message text is too long. Maximum length is {Defaults.MaxMessageLength} characters.");
                    return View("Create", viewModel);
                }

                if (postCreateRm.MessageHtml.Length > Defaults.MaxMessageHtmlLength)
                {
                    ModelState.AddModelError(nameof(viewModel.Message), $"Resulting HTML is too long. Maximum length is {Defaults.MaxMessageHtmlLength} characters.");
                    return View("Create", viewModel);
                }

                var postCreateResult = await _postService.CreatePostAsync(postCreateRm, viewModel.Attachments ?? new FormFileCollection(), cancellationToken);

                var actionResult = postCreateResult.Match<IActionResult>(
                    success =>
                    {
                        _logger.LogDebug(
                            LogEventIds.PostCreated,
                            "Post created. ThreadId: {ThreadId}, PostId: {PostId}, CategoryAlias: {CategoryAlias}",
                            viewModel.ThreadId,
                            success,
                            viewModel.CategoryAlias);

                        return Redirect(
                            Url.RouteUrl(
                                "ThreadDetails",
                                new
                                {
                                    categoryAlias = viewModel.CategoryAlias,
                                    threadId = viewModel.ThreadId,
                                }) + "#" + success);
                    },
                    err =>
                    {
                        _logger.LogError(
                            LogEventIds.PostCreateError,
                            "Error creating post. CategoryAlias: {CategoryAlias}, ThreadId: {ThreadId}, Error: {Error}",
                            viewModel.CategoryAlias,
                            viewModel.ThreadId,
                            err.ErrorMessage);

                        ViewBag.ErrorMessage = err.ErrorMessage;
                        return View("Create", viewModel);
                    });

                return actionResult;
            }
            catch (Exception e)
            {
                _logger.LogError(
                    LogEventIds.PostCreateError,
                    e,
                    "Error creating post in category '{CategoryAlias}'. ThreadId: {ThreadId}, Message length: {MessageLength}, AttachmentsCount: {AttachmentsCount}",
                    categoryAlias,
                    viewModel.ThreadId,
                    viewModel.Message.Length,
                    viewModel.Attachments?.Count);

                ViewBag.ErrorMessage = "Error occurred while creating a post. Please try again.";
                return View("Create", viewModel);
            }
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View("Create", viewModel);
        }
    }

    [HttpGet("search", Name = "PostSearch")]
    public async Task<IActionResult> Search(
        [Required] [FromQuery] SearchRequestViewModel request,
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int size = 10,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToRoute("HomeIndex", new { message = ModelState.ModelErrorsToString() });
        }
        else
        {
            var filter = new SearchPostsPagingFilter
            {
                PageSize = size,
                PageNumber = page,
                OrderBy = new List<OrderByItem>
                {
                    new() { Field = nameof(Post.CreatedAt), Direction = OrderByDirection.Desc },
                    new() { Field = nameof(Post.Id), Direction = OrderByDirection.Desc },
                },
                SearchQuery = request.Query,
            };

            var result = await _postService.SearchPostsPaginatedAsync(filter, cancellationToken);

            var posts = new PagedResult<PostDetailsViewModel>(result.Data.ToViewModels(), filter, result.TotalItemCount);

            var vm = new PostSearchResultViewModel
            {
                Query = request.Query,
                Posts = posts,
            };

            return View(vm);
        }
    }
}
