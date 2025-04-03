using System;
using System.Threading;
using System.Threading.Tasks;
using DNTCaptcha.Core;
using Hikkaba.Shared.Enums;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Application.Contracts;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Services.Contracts;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc;

[Authorize]
public class ThreadsController : BaseMvcController
{
    private readonly ILogger<ThreadsController> _logger;
    private readonly IMessagePostProcessor _messagePostProcessor;
    private readonly ICategoryService _categoryService;
    private readonly IThreadService _threadService;

    public ThreadsController(
        ILogger<ThreadsController> logger,
        UserManager<ApplicationUser> userManager,
        IMessagePostProcessor messagePostProcessor,
        ICategoryService categoryService,
        IThreadService threadService) : base(userManager)
    {
        _logger = logger;
        _messagePostProcessor = messagePostProcessor;
        _categoryService = categoryService;
        _threadService = threadService;
    }

    [Route("{categoryAlias}/Threads/{threadId:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(string categoryAlias, long threadId, CancellationToken cancellationToken)
    {
        var threadPosts = await _threadService.GetThreadDetailsAsync(threadId, cancellationToken);

        if (threadPosts is null)
        {
            return NotFound();
        }

        var threadDetailsViewModel = threadPosts.ToViewModel();

        return View(threadDetailsViewModel);
    }

    [Route("{categoryAlias}/Threads/Create")]
    [AllowAnonymous]
    public async Task<IActionResult> Create(string categoryAlias, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetAsync(categoryAlias, false, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        var threadAnonymousCreateViewModel = new ThreadAnonymousCreateViewModel
        {
            Title = string.Empty,
            Message = string.Empty,
            Attachments = new FormFileCollection(),
            CategoryAlias = category.Alias,
            CategoryName = category.Name,
        };
        return View(threadAnonymousCreateViewModel);
    }

    [Route("{categoryAlias}/Threads/Create")]
    [HttpPost]
    [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Create(string categoryAlias, ThreadAnonymousCreateViewModel viewModel, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var category = await _categoryService.GetAsync(categoryAlias, false, cancellationToken);
                if (category is null)
                {
                    return NotFound();
                }

                var threadCreateRm = new ThreadCreateRequestModel
                {
                    CategoryAlias = category.Alias,
                    ThreadTitle = viewModel.Title,
                    BlobContainerId = Guid.NewGuid(),
                    IsSageEnabled = false,
                    MessageHtml = _messagePostProcessor.MessageToSafeHtml(category.Alias, null, viewModel.Message),
                    MessageText = _messagePostProcessor.MessageToPlainText(viewModel.Message),
                    UserIpAddress = UserIpAddressBytes,
                    UserAgent = UserAgent,
                };

                var createThreadResult = await _threadService.CreateThreadAsync(threadCreateRm, viewModel.Attachments, cancellationToken);

                _logger.LogDebug(LogEventIds.ThreadCreated, "Thread created. ThreadId: {ThreadId}, CategoryAlias: {CategoryAlias}", createThreadResult.ThreadId, category.Alias);

                return RedirectToAction(
                    "Details",
                    "Threads",
                    new
                    {
                        categoryAlias = category.Alias,
                        threadId = createThreadResult.ThreadId,
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    LogEventIds.ThreadCreateError,
                    ex,
                    "Error creating thread in category '{CategoryAlias}'. Title: '{Title}', Message length: {MessageLength}, Attachments count: {AttachmentsCount}",
                    categoryAlias,
                    viewModel.Title,
                    viewModel.Message.Length,
                    viewModel.Attachments.Count);

                ViewBag.ErrorMessage = "Error occurred while creating a thread. Please try again.";
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
    [Route("{categoryAlias}/Threads/{threadId}/Edit")]
    public async Task<IActionResult> Edit(string categoryAlias, long threadId)
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
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
        }
    }

    [Route("{categoryAlias}/Threads/{threadId}/Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string categoryAlias, long threadId, ThreadEditViewModel threadEditViewModel)
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
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetIsPinned(long threadId, bool isPinned)
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
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetIsClosed(long threadId, bool isClosed)
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
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetIsDeleted(long threadId, bool isDeleted)
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
            throw new HikkabaHttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
        }
    }
    */
}
