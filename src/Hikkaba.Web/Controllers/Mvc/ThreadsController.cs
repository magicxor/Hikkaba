using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using DNTCaptcha.Core;
using DNTCaptcha.Core.Providers;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Exceptions;
using Hikkaba.Service;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Filters;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc
{
    // todo: add moderation buttons: delete post, add notice

    [TypeFilter(typeof(ExceptionLoggingFilter))]
    [Authorize]
    public class ThreadsController : BaseMvcController
    {
        private readonly ILogger<ThreadsController> _logger;
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;
        private readonly ICategoryToModeratorService _categoryToModeratorService;

        public ThreadsController(
            UserManager<ApplicationUser> userManager,
            ILogger<ThreadsController> logger,
            IMapper mapper,
            ICategoryService categoryService,
            IThreadService threadService,
            IPostService postService,
            ICategoryToModeratorService categoryToModeratorService) : base(userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _categoryService = categoryService;
            _threadService = threadService;
            _postService = postService;
            _categoryToModeratorService = categoryToModeratorService;
        }

        [Route("{categoryAlias}/Threads/{threadId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(string categoryAlias, Guid threadId)
        {
            var threadDto = await _threadService.GetAsync(threadId);
            var categoryDto = await _categoryService.GetAsync(categoryAlias);

            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                                                .IsUserCategoryModerator(threadDto.CategoryId, User);
            if (threadDto.IsDeleted && (!isCurrentUserCategoryModerator))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound, $"Thread {threadId} not found.");
            }

            if (threadDto.CategoryId != categoryDto.Id)
            {
                categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                return RedirectToAction("Details", new {categoryAlias = categoryDto.Alias, threadId = threadDto.Id});
            }

            var postDtoList =
                await
                    _postService.ListAsync(post => (!post.IsDeleted) && (post.Thread.Id == threadId),
                        post => post.Created);

            var postDetailsViewModels = _mapper.Map<IList<PostDetailsViewModel>>(postDtoList);
            foreach (var postDetailsViewModel in postDetailsViewModels)
            {
                postDetailsViewModel.ThreadShowThreadLocalUserHash = threadDto.ShowThreadLocalUserHash;
                postDetailsViewModel.CategoryAlias = categoryDto.Alias;
                postDetailsViewModel.CategoryId = categoryDto.Id;
                postDetailsViewModel.Answers = new List<Guid>(
                    postDetailsViewModels
                        .Where(answer => answer.Message.Contains(">>"+ postDetailsViewModel.Id.ToString()))
                        .Select(answer => answer.Id));
            }

            var threadDetailsViewModel = _mapper.Map<ThreadDetailsViewModel>(threadDto);
            threadDetailsViewModel.PostCount = postDetailsViewModels.Count;
            threadDetailsViewModel.CategoryAlias = categoryDto.Alias;
            threadDetailsViewModel.CategoryName = categoryDto.Name;
            threadDetailsViewModel.Posts = postDetailsViewModels;

            return View(threadDetailsViewModel);
        }

        [Route("{categoryAlias}/Threads/Create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create(string categoryAlias)
        {
            var category = await _categoryService.GetAsync(categoryAlias);
            var threadAnonymousCreateViewModel = new ThreadAnonymousCreateViewModel()
            {
                CategoryAlias = category.Alias,
                CategoryName = category.Name,
            };
            return View(threadAnonymousCreateViewModel);
        }

        [Route("{categoryAlias}/Threads/Create")]
        [HttpPost]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.",
             IsNumericErrorMessage = "The input value should be a number.",
             CaptchaGeneratorLanguage = Language.English)]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create(string categoryAlias,
            ThreadAnonymousCreateViewModel threadAnonymousCreateViewModel)
        {
            if (ModelState.IsValid)
            {
                var category = await _categoryService.GetAsync(categoryAlias);

                var threadDto = _mapper.Map<ThreadDto>(threadAnonymousCreateViewModel);
                threadDto.BumpLimit = category.DefaultBumpLimit;
                threadDto.ShowThreadLocalUserHash = category.DefaultShowThreadLocalUserHash;
                threadDto.CategoryId = category.Id;

                var threadId = await _threadService.CreateAsync(threadDto);

                var postDto = _mapper.Map<PostDto>(threadAnonymousCreateViewModel);
                postDto.ThreadId = threadId;
                postDto.UserIpAddress = UserIpAddress.ToString();
                postDto.UserAgent = UserAgent;

                try
                {
                    var postId = await _postService.CreateAsync(threadAnonymousCreateViewModel.Attachments, postDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Can't create new post due to exception: {ex}. Thread creation failed.");
                    await _threadService.DeleteAsync(threadId);
                    throw;
                }

                return RedirectToAction("Details", "Threads", new { categoryAlias = categoryAlias, threadId = threadId });
            }
            else
            {
                ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
                return await Create(categoryAlias);
            }
        }

        [Route("{categoryAlias}/Threads/{threadId}/Edit")]
        public IActionResult Edit(string categoryAlias, Guid threadId)
        {
            throw new NotImplementedException();
        }

        [Route("{categoryAlias}/Threads/{threadId}/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string categoryAlias, Guid threadId, ThreadEditViewModel threadEditViewModel)
        {
            throw new NotImplementedException();
        }

        [Route("{categoryAlias}/Threads/{threadId}/Delete")]
        public IActionResult Delete(string categoryAlias, Guid threadId)
        {
            throw new NotImplementedException();
        }

        [Route("{categoryAlias}/Threads/{threadId}/Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string categoryAlias, Guid threadId, ThreadEditViewModel threadEditViewModel)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleIsPinnedOption(Guid threadId)
        {
            var threadDto = await _threadService.GetAsync(threadId);
            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                                                .IsUserCategoryModerator(threadDto.CategoryId, User);
            if (isCurrentUserCategoryModerator)
            {
                threadDto.IsPinned = !threadDto.IsPinned;
                await _threadService.EditAsync(threadDto, CurrentUserId);
                var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                return RedirectToAction("Details", "Threads", new { categoryAlias = categoryDto.Alias, threadId = threadDto.Id });
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleIsClosedOption(Guid threadId)
        {
            var threadDto = await _threadService.GetAsync(threadId);
            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                                                .IsUserCategoryModerator(threadDto.CategoryId, User);
            if (isCurrentUserCategoryModerator)
            {
                threadDto.IsClosed = !threadDto.IsClosed;
                await _threadService.EditAsync(threadDto, CurrentUserId);
                var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                return RedirectToAction("Details", "Threads", new { categoryAlias = categoryDto.Alias, threadId = threadDto.Id });
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleIsDeletedOption(Guid threadId)
        {
            var threadDto = await _threadService.GetAsync(threadId);
            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                                                .IsUserCategoryModerator(threadDto.CategoryId, User);
            if (isCurrentUserCategoryModerator)
            {
                threadDto.IsDeleted = !threadDto.IsDeleted;
                await _threadService.EditAsync(threadDto, CurrentUserId);
                var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                return RedirectToAction("Details", "Categories", new {categoryAlias = categoryDto.Alias});
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }
    }
}