using TPrimaryKey = System.Guid;
using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using DNTCaptcha.Core;
using DNTCaptcha.Core.Providers;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Extensions;
using Hikkaba.Models.Dto;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Exceptions;
using Hikkaba.Services;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hikkaba.Web.Utils;

namespace Hikkaba.Web.Controllers.Mvc
{
    [Authorize]
    public class ThreadsController : BaseMvcController
    {
        private readonly ILogger<ThreadsController> _logger;
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IThreadService _threadService;
        private readonly ICategoryToModeratorService _categoryToModeratorService;

        public ThreadsController(
            UserManager<ApplicationUser> userManager,
            ILogger<ThreadsController> logger,
            IMapper mapper,
            ICategoryService categoryService,
            IThreadService threadService,
            ICategoryToModeratorService categoryToModeratorService) : base(userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _categoryService = categoryService;
            _threadService = threadService;
            _categoryToModeratorService = categoryToModeratorService;
        }

        [Route("{categoryAlias}/Threads/{threadId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(string categoryAlias, TPrimaryKey threadId)
        {
            var aggregationDto = await _threadService.GetAggregationAsync(threadId, User);
            var threadDetailsViewModel = _mapper.Map<ThreadDetailsViewModel>(aggregationDto);
            return View(threadDetailsViewModel);
        }

        [Route("{categoryAlias}/Threads/Create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create(string categoryAlias)
        {
            var category = await _categoryService.GetAsync(categoryAlias);
            var threadAnonymousCreateViewModel = new ThreadAnonymousCreateViewModel
            {
                CategoryAlias = category.Alias,
                CategoryName = category.Name,
            };
            return View(threadAnonymousCreateViewModel);
        }

        [Route("{categoryAlias}/Threads/Create")]
        [HttpPost]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number",
            CaptchaGeneratorDisplayMode = DisplayMode.ShowDigits,
            CaptchaGeneratorLanguage = Language.English)]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create(ThreadAnonymousCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var categoryDto = await _categoryService.GetAsync(viewModel.CategoryAlias);
                    
                    var threadDto = _mapper.Map<ThreadDto>(viewModel);
                    threadDto.BumpLimit = categoryDto.DefaultBumpLimit;
                    threadDto.ShowThreadLocalUserHash = categoryDto.DefaultShowThreadLocalUserHash;
                    threadDto.CategoryId = categoryDto.Id;
                    
                    var postDto = _mapper.Map<PostDto>(viewModel);
                    postDto.UserIpAddress = UserIpAddress.ToString();
                    postDto.UserAgent = UserAgent;
                    
                    var threadCreateDto = new ThreadPostCreateDto
                    {
                        Category = categoryDto,
                        Thread = threadDto,
                        Post = postDto,
                    };
                    
                    var createResultDto = await _threadService.CreateThreadPostAsync(viewModel.Attachments, threadCreateDto, true);
                    return RedirectToAction("Details", "Threads", new { categoryAlias = viewModel.CategoryAlias, threadId = createResultDto.ThreadId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(EventIdentifiers.ThreadCreateError.ToEventId(), ex, $"Can't create new thread due to exception");
                    
                    ViewBag.ErrorMessage = "Can't create new thread";
                    return View(viewModel);
                }
            }
            else
            {
                ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
                return View(viewModel);
            }
        }

        [Route("{categoryAlias}/Threads/{threadId}/Edit")]
        public async Task<IActionResult> Edit(string categoryAlias, TPrimaryKey threadId)
        {
            var threadDto = await _threadService.GetAsync(threadId);
            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                                                .IsUserCategoryModeratorAsync(threadDto.CategoryId, User);
            if (isCurrentUserCategoryModerator)
            {
                var threadEditViewModel = _mapper.Map<ThreadEditViewModel>(threadDto);
                return View(threadEditViewModel);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }

        [Route("{categoryAlias}/Threads/{threadId}/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string categoryAlias, TPrimaryKey threadId, ThreadEditViewModel threadEditViewModel)
        {
            var threadDto = await _threadService.GetAsync(threadId);
            _mapper.Map(threadEditViewModel, threadDto);
            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                                                .IsUserCategoryModeratorAsync(threadDto.CategoryId, User);
            if (isCurrentUserCategoryModerator)
            {
                await _threadService.EditAsync(threadDto);
                return RedirectToAction("Details", "Threads", new { categoryAlias = categoryAlias, threadId = threadDto.Id });
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetIsPinned(TPrimaryKey threadId, bool isPinned)
        {
            var threadDto = await _threadService.GetAsync(threadId);
            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                .IsUserCategoryModeratorAsync(threadDto.CategoryId, User);
            if (isCurrentUserCategoryModerator)
            {
                var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                await _threadService.SetIsPinnedAsync(threadId, isPinned);
                return RedirectToAction("Details", "Threads", new { categoryAlias = categoryDto.Alias, threadId = threadDto.Id });
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetIsClosed(TPrimaryKey threadId, bool isClosed)
        {
            var threadDto = await _threadService.GetAsync(threadId);
            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                .IsUserCategoryModeratorAsync(threadDto.CategoryId, User);
            if (isCurrentUserCategoryModerator)
            {
                var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                await _threadService.SetIsClosedAsync(threadId, isClosed);
                return RedirectToAction("Details", "Threads", new { categoryAlias = categoryDto.Alias, threadId = threadDto.Id });
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetIsDeleted(TPrimaryKey threadId, bool isDeleted)
        {
            var threadDto = await _threadService.GetAsync(threadId);
            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                .IsUserCategoryModeratorAsync(threadDto.CategoryId, User);
            if (isCurrentUserCategoryModerator)
            {
                var categoryDto = await _categoryService.GetAsync(threadDto.CategoryId);
                await _threadService.SetIsDeletedAsync(threadId, isDeleted);
                return RedirectToAction("Details", "Threads", new { categoryAlias = categoryDto.Alias, threadId = threadDto.Id });
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
            }
        }
    }
}