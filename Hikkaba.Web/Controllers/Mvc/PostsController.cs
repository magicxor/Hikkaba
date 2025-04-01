using System;
using System.Threading;
using System.Threading.Tasks;
using DNTCaptcha.Core;
using Hikkaba.Shared.Enums;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hikkaba.Web.Utils;
using Hikkaba.Application.Contracts;
using Hikkaba.Web.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc;

[Authorize]
public class PostsController : BaseMvcController
{
    private readonly ILogger<PostsController> _logger;
    private readonly IMessagePostProcessor _messagePostProcessor;
    private readonly ICategoryService _categoryService;
    private readonly IThreadService _threadService;
    private readonly IPostService _postService;

    public PostsController(
        ILogger<PostsController> logger,
        UserManager<ApplicationUser> userManager,
        IMessagePostProcessor messagePostProcessor,
        ICategoryService categoryService,
        IThreadService threadService,
        IPostService postService) : base(userManager)
    {
        _logger = logger;
        _messagePostProcessor = messagePostProcessor;
        _categoryService = categoryService;
        _threadService = threadService;
        _postService = postService;
    }

    [Route("{categoryAlias}/Threads/{threadId:long}/Posts/Create")]
    [AllowAnonymous]
    public async Task<IActionResult> Create(string categoryAlias, long threadId)
    {
        var category = await _categoryService.GetAsync(categoryAlias, false);
        if (category is null)
        {
            return NotFound();
        }

        var thread = await _threadService.GetThreadAsync(threadId);
        var postAnonymousCreateViewModel = new PostAnonymousCreateViewModel
        {
            IsSageEnabled = false,
            Message = string.Empty,
            Attachments = new FormFileCollection(),
            CategoryAlias = category.Alias,
            CategoryName = category.Name,
            ThreadId = thread.Id,
        };
        return View(postAnonymousCreateViewModel);
    }

    [Route("{categoryAlias}/Threads/{threadId:long}/Posts/Create")]
    [HttpPost]
    [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Create(string categoryAlias, long threadId, PostAnonymousCreateViewModel viewModel, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var category = await _categoryService.GetAsync(viewModel.CategoryAlias, false);
                if (category is null)
                {
                    return NotFound();
                }

                var postCreateRm = new PostCreateRequestModel
                {
                    BlobContainerId = Guid.NewGuid(),
                    IsSageEnabled = viewModel.IsSageEnabled,
                    MessageHtml = _messagePostProcessor.MessageToSafeHtml(category.Alias, viewModel.ThreadId, viewModel.Message),
                    MessageText = _messagePostProcessor.MessageToPlainText(viewModel.Message),
                    UserIpAddress = UserIpAddressBytes,
                    UserAgent = UserAgent,
                    ThreadId = viewModel.ThreadId,
                    MentionedPosts = _messagePostProcessor.GetMentionedPosts(viewModel.Message),
                };

                var postId = await _postService.CreatePostAsync(postCreateRm, viewModel.Attachments ?? new FormFileCollection(), cancellationToken);

                _logger.LogDebug(LogEventIds.PostCreated, "Post created. PostId: {PostId}, ThreadId: {ThreadId}, CategoryAlias: {CategoryAlias}", postId, viewModel.ThreadId, viewModel.CategoryAlias);

                return Redirect(
                    Url.Action("Details",
                        "Threads",
                        new
                        {
                            categoryAlias = viewModel.CategoryAlias,
                            threadId = viewModel.ThreadId,
                        }) + "#" + postId);
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
                return View(viewModel);
            }
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }

    /*
    [Route("Search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search(SearchRequestViewModel request, int page = 1, int size = 10)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Details", "Error", new { message = ModelState.ModelErrorsToString() });
        }
        else
        {
            var latestPostsDtoList = await _postService
                .ListPostsAsync(new PostSearchPagingFilter
                {
                    PageNumber = page,
                    PageSize = size,
                    OrderBy = [new OrderByItem { Field = nameof(Post.CreatedAt), Direction = OrderByDirection.Desc }],
                    SearchQuery = request.Query,
                });
            var latestPostDetailsViewModels = _mapper.Map<List<PostDetailsViewModel>>(latestPostsDtoList.Data);
            foreach (var latestPostDetailsViewModel in latestPostDetailsViewModels)
            {
                var threadDto = await _threadService.GetThreadAsync(latestPostDetailsViewModel.ThreadId);
                var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                latestPostDetailsViewModel.ThreadShowThreadLocalUserHash = threadDto.ShowThreadLocalUserHash;
                latestPostDetailsViewModel.CategoryAlias = categoryDto.Alias;
            }

            var searchResultViewModel = new PostSearchResultViewModel
            {
                Query = query,
                Posts = new PagedResult<PostDetailsViewModel>
                {
                    CurrentPage = pageDto,
                    CurrentPageItems = latestPostDetailsViewModels,
                    TotalItemsCount = latestPostsDtoList.TotalItemCount,
                },
            };
            return View(searchResultViewModel);
        }
    }

/*
    [Route("{categoryAlias}/Threads/{threadId}/Posts/{postId}/Edit")]
    public async Task<IActionResult> Edit(string categoryAlias, long threadId, long postId)
    {
        var postDto = await _postService.GetPostAsync(postId);
        var threadDto = await _threadService.GetAsync(postDto.ThreadId);
        var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);

        if ((threadDto.Id != threadId) || (categoryDto.Alias != categoryAlias))
        {
            return RedirectToAction("Edit", new
            {
                categoryAlias = categoryDto.Alias,
                threadId = threadDto.Id,
                postId = postDto.Id,
            });
        }

        var isCurrentUserCategoryModerator = await _categoryToModeratorService
            .IsUserCategoryModeratorAsync(threadDto.CategoryId, User);
        if (isCurrentUserCategoryModerator)
        {
            var postEditViewModel = _mapper.Map<PostEditViewModel>(postDto);
            postEditViewModel.CategoryAlias = categoryDto.Alias;
            return View(postEditViewModel);
        }
        else
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
        }
    }

    [Route("{categoryAlias}/Threads/{threadId}/Posts/{postId}/Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PostEditViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var postDto = await _postService.GetPostAsync(viewModel.Id);
            var threadDto = await _threadService.GetAsync(postDto.ThreadId);
            var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);

            if ((threadDto.Id != viewModel.ThreadId) || (categoryDto.Alias != viewModel.CategoryAlias))
            {
                return RedirectToAction("Edit", new
                {
                    categoryAlias = categoryDto.Alias,
                    threadId = threadDto.Id,
                    postId = postDto.Id,
                });
            }

            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                .IsUserCategoryModeratorAsync(threadDto.CategoryId, User);
            if (isCurrentUserCategoryModerator)
            {
                postDto = _mapper.Map(viewModel, postDto);
                await _postService.EditPostAsync(postDto);
                return RedirectToAction("Details", "Threads", new { categoryAlias = categoryDto.Alias, threadId = threadDto.Id });
            }
            else
            {
                throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetIsDeleted(long postId, bool isDeleted)
    {
        var postDto = await _postService.GetPostAsync(postId);
        var threadDto = await _threadService.GetAsync(postDto.ThreadId);
        var isCurrentUserCategoryModerator = await _categoryToModeratorService
            .IsUserCategoryModeratorAsync(threadDto.CategoryId, User);
        if (isCurrentUserCategoryModerator)
        {
            var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
            await _postService.SetPostDeletedAsync(postId, isDeleted);
            return RedirectToAction("Details", "Threads", new { categoryAlias = categoryDto.Alias, threadId = threadDto.Id });
        }
        else
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
        }
    }
    */
}
