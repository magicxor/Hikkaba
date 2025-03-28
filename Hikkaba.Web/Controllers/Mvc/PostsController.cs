using System.Net;
using System.Threading.Tasks;
using Hikkaba.Common.Exceptions;
using Hikkaba.Data.Entities;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hikkaba.Web.Utils;
using Hikkaba.Services.Contracts;
using Hikkaba.Web.Services.Contracts;

namespace Hikkaba.Web.Controllers.Mvc;

[Authorize]
public class PostsController : BaseMvcController
{
    private readonly IMessagePostProcessor _messagePostProcessor;
    private readonly ICategoryService _categoryService;
    private readonly IThreadService _threadService;
    private readonly IPostService _postService;

    public PostsController(
        UserManager<ApplicationUser> userManager,
        IMessagePostProcessor messagePostProcessor,
        ICategoryService categoryService,
        IThreadService threadService,
        IPostService postService) : base(userManager)
    {
        _messagePostProcessor = messagePostProcessor;
        _categoryService = categoryService;
        _threadService = threadService;
        _postService = postService;
    }

    /*
    [Route("{categoryAlias}/Threads/{threadId}/Posts/Create")]
    [AllowAnonymous]
    public async Task<IActionResult> Create(string categoryAlias, long threadId)
    {
        var category = await _categoryService.GetAsync(categoryAlias);
        var thread = await _threadService.GetAsync(threadId);
        var postAnonymousCreateViewModel = new PostAnonymousCreateViewModel
        {
            CategoryAlias = category.Alias,
            ThreadId = thread.Id,
        };
        return View(postAnonymousCreateViewModel);
    }

    [Route("{categoryAlias}/Threads/{threadId}/Posts/Create")]
    [HttpPost]
    [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Create(PostAnonymousCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var categoryDto = await _categoryService.GetAsync(viewModel.CategoryAlias);
            var threadDto = await _threadService.GetAsync(viewModel.ThreadId);
            if (!threadDto.IsClosed)
            {
                var postDto = _mapper.Map<PostDto>(viewModel);
                postDto.UserIpAddress = UserIpAddress.ToString();
                postDto.UserAgent = UserAgent;

                var threadPostCreateDto = new ThreadPostCreateSm
                {
                    Category = categoryDto,
                    Thread = threadDto,
                    Post = postDto,
                };

                var createResultDto = await _threadService.CreateThreadPostAsync(viewModel.Attachments, threadPostCreateDto, false);
                return Redirect(Url.Action("Details", "Threads",
                    new
                    {
                        categoryAlias = viewModel.CategoryAlias,
                        threadId = createResultDto.ThreadId,
                    }) + "#" + createResultDto.PostId);
            }
            else
            {
                throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Thread {threadDto.Id} is closed.");
            }
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }

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
                var threadDto = await _threadService.GetAsync(latestPostDetailsViewModel.ThreadId);
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
