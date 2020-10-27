using TPrimaryKey = System.Guid;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using DNTCaptcha.Core;
using DNTCaptcha.Core.Providers;
using Hikkaba.Models.Dto;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Exceptions;
using Hikkaba.Services;
using Hikkaba.Services.Base.Generic;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Web.ViewModels.SearchViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hikkaba.Web.Utils;
using Hikkaba.Models.Enums;

namespace Hikkaba.Web.Controllers.Mvc
{
    [Authorize]
    public class PostsController : BaseMvcController
    {
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;
        private readonly ICategoryToModeratorService _categoryToModeratorService;

        public PostsController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ICategoryService categoryService,
            IThreadService threadService,
            IPostService postService,
            ICategoryToModeratorService categoryToModeratorService) : base(userManager)
        {
            _mapper = mapper;
            _categoryService = categoryService;
            _threadService = threadService;
            _postService = postService;
            _categoryToModeratorService = categoryToModeratorService;
        }

        [Route("{categoryAlias}/Threads/{threadId}/Posts/Create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create(string categoryAlias, TPrimaryKey threadId)
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
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number",
            CaptchaGeneratorDisplayMode = DisplayMode.ShowDigits,
            CaptchaGeneratorLanguage = Language.English)]
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
                    
                    var threadPostCreateDto = new ThreadPostCreateDto
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
                    throw new HttpResponseException(HttpStatusCode.Forbidden, $"Thread {threadDto.Id} is closed.");
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
                var pageDto = new PageDto(page, size);
                var query = request.Query;
                var latestPostsDtoList = await _postService
                                           .PagedListAsync(
                                               post =>
                                                    !post.IsDeleted
                                                    && !post.Thread.IsDeleted
                                                    && !post.Thread.Category.IsDeleted
                                                    && (post.Message.Contains(query) 
                                                        || (post.Thread.Title.Contains(query) && post == post.Thread.Posts.OrderBy(tp => tp.Created).FirstOrDefault())
                                                        ),
                                               post => post.Created,
                                               AdditionalRecordType.None,
                                               true,
                                               pageDto);
                var latestPostDetailsViewModels = _mapper.Map<List<PostDetailsViewModel>>(latestPostsDtoList.CurrentPageItems);
                foreach (var latestPostDetailsViewModel in latestPostDetailsViewModels)
                {
                    var threadDto = await _threadService.GetAsync(latestPostDetailsViewModel.ThreadId);
                    var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                    latestPostDetailsViewModel.ThreadShowThreadLocalUserHash = threadDto.ShowThreadLocalUserHash;
                    latestPostDetailsViewModel.CategoryAlias = categoryDto.Alias;
                }

                var searchResultViewModel = new SearchResultViewModel
                {
                    Query = query,
                    Posts = new BasePagedList<PostDetailsViewModel>
                    {
                        CurrentPage = pageDto,
                        CurrentPageItems = latestPostDetailsViewModels,
                        TotalItemsCount = latestPostsDtoList.TotalItemsCount
                    },
                };
                return View(searchResultViewModel);
            }
        }

        [Route("{categoryAlias}/Threads/{threadId}/Posts/{postId}/Edit")]
        public async Task<IActionResult> Edit(string categoryAlias, TPrimaryKey threadId, TPrimaryKey postId)
        {
            var postDto = await _postService.GetAsync(postId);
            var threadDto = await _threadService.GetAsync(postDto.ThreadId);
            var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);

            if ((threadDto.Id != threadId) || (categoryDto.Alias != categoryAlias))
            {
                return RedirectToAction("Edit", new
                {
                    categoryAlias = categoryDto.Alias,
                    threadId = threadDto.Id,
                    postId = postDto.Id
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
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }

        [Route("{categoryAlias}/Threads/{threadId}/Posts/{postId}/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var postDto = await _postService.GetAsync(viewModel.Id);
                var threadDto = await _threadService.GetAsync(postDto.ThreadId);
                var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);

                if ((threadDto.Id != viewModel.ThreadId) || (categoryDto.Alias != viewModel.CategoryAlias))
                {
                    return RedirectToAction("Edit", new
                    {
                        categoryAlias = categoryDto.Alias,
                        threadId = threadDto.Id,
                        postId = postDto.Id
                    });
                }

                var isCurrentUserCategoryModerator = await _categoryToModeratorService
                                                    .IsUserCategoryModeratorAsync(threadDto.CategoryId, User);
                if (isCurrentUserCategoryModerator)
                {
                    postDto = _mapper.Map(viewModel, postDto);
                    await _postService.EditAsync(postDto);
                    return RedirectToAction("Details", "Threads", new { categoryAlias = categoryDto.Alias, threadId = threadDto.Id });
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
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
        public async Task<IActionResult> SetIsDeleted(TPrimaryKey postId, bool isDeleted)
        {
            var postDto = await _postService.GetAsync(postId);
            var threadDto = await _threadService.GetAsync(postDto.ThreadId);
            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                                                .IsUserCategoryModeratorAsync(threadDto.CategoryId, User);
            if (isCurrentUserCategoryModerator)
            {
                var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                await _postService.SetIsDeletedAsync(postId, isDeleted);
                return RedirectToAction("Details", "Threads", new { categoryAlias = categoryDto.Alias, threadId = threadDto.Id });
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }
    }
}
