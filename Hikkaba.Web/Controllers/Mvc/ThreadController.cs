using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using DNTCaptcha.Core;
using Hikkaba.Shared.Enums;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Implementations;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Extensions;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Mappings;
using Hikkaba.Web.Services.Contracts;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCSharp.HttpUserAgentParser.AspNetCore;

namespace Hikkaba.Web.Controllers.Mvc;

[AllowAnonymous]
[Route("threads")]
public sealed class ThreadController : BaseMvcController
{
    private readonly ILogger<ThreadController> _logger;
    private readonly IMessagePostProcessor _messagePostProcessor;
    private readonly ICategoryService _categoryService;
    private readonly IThreadService _threadService;
    private readonly GeoIpCountryReader _geoIpCountryReader;
    private readonly IHttpUserAgentParserAccessor _httpUserAgentParserAccessor;

    public ThreadController(
        ILogger<ThreadController> logger,
        IMessagePostProcessor messagePostProcessor,
        ICategoryService categoryService,
        IThreadService threadService,
        GeoIpCountryReader geoIpCountryReader,
        IHttpUserAgentParserAccessor httpUserAgentParserAccessor)
    {
        _logger = logger;
        _messagePostProcessor = messagePostProcessor;
        _categoryService = categoryService;
        _threadService = threadService;
        _geoIpCountryReader = geoIpCountryReader;
        _httpUserAgentParserAccessor = httpUserAgentParserAccessor;
    }

    [HttpGet("/{categoryAlias}/{threadId:long}", Name = "ThreadDetails")]
    public async Task<IActionResult> Details(
        [Required] [FromRoute] [MaxLength(Defaults.MaxCategoryAliasLength)]
        string categoryAlias,
        [Required] [FromRoute] [Range(1, long.MaxValue)]
        long threadId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        var threadPosts = await _threadService.GetThreadDetailsAsync(threadId, cancellationToken);

        if (threadPosts is null)
        {
            var returnUrl = GetLocalReferrerOrRoute("CategoryDetails", new { categoryAlias });
            return CustomErrorPage(
                StatusCodes.Status404NotFound,
                "The requested thread was not found.",
                returnUrl);
        }

        var threadDetailsViewModel = threadPosts.ToViewModel();

        return View(threadDetailsViewModel);
    }

    [HttpGet("/{categoryAlias}/create", Name = "ThreadCreate")]
    public async Task<IActionResult> Create(
        [Required] [FromRoute] [MaxLength(Defaults.MaxCategoryAliasLength)]
        string categoryAlias,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.ModelErrorsToString();
            ViewBag.ErrorMessage = errorMessage;
            return CustomErrorPage(StatusCodes.Status400BadRequest, errorMessage, GetLocalReferrerOrNull());
        }

        var category = await _categoryService.GetCategoryAsync(categoryAlias, false, cancellationToken);
        if (category is null)
        {
            var returnUrl = GetLocalReferrerOrRoute("HomeIndex");
            return CustomErrorPage(
                StatusCodes.Status404NotFound,
                "The requested category was not found.",
                returnUrl);
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

    [HttpPost("/{categoryAlias}/create", Name = "ThreadCreateConfirm")]
    [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirm(
        [Required] [FromRoute] [MaxLength(Defaults.MaxCategoryAliasLength)]
        string categoryAlias,
        [Required] ThreadAnonymousCreateViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View("Create", viewModel);
        }

        try
        {
            string? countryIsoCode = null;
            var userIp = Request.HttpContext.Connection.RemoteIpAddress;
            if (userIp is not null && _geoIpCountryReader.TryCountry(userIp, out var countryResponse))
            {
                countryIsoCode = countryResponse?.Country.IsoCode;
            }

            var userAgentInfo = _httpUserAgentParserAccessor.Get(HttpContext);
            var clientInfoModel = new ClientInfoModel
            {
                CountryIsoCode = countryIsoCode,
                BrowserType = userAgentInfo?.Name,
                OsType = userAgentInfo?.Platform?.PlatformType is { } pt
                    ? Enum.GetName(pt)
                    : null,
            };

            var messagePlainText = _messagePostProcessor.MessageToPlainText(viewModel.Message);
            var threadTitle = string.IsNullOrWhiteSpace(viewModel.Title)
                ? messagePlainText.Cut(Defaults.MaxTitleLength)
                : viewModel.Title.Cut(Defaults.MaxTitleLength);

            var threadCreateRm = new ThreadCreateRequestModel
            {
                CategoryAlias = viewModel.CategoryAlias,
                ThreadTitle = threadTitle,
                BlobContainerId = Guid.NewGuid(),
                MessageHtml = _messagePostProcessor.MessageToSafeHtml(viewModel.CategoryAlias, null, viewModel.Message),
                MessageText = messagePlainText,
                UserIpAddress = UserIpAddressBytes,
                UserAgent = UserAgent,
                ClientInfo = clientInfoModel,
            };

            if (threadCreateRm.MessageText.Length > Defaults.MaxMessageTextLength)
            {
                ModelState.AddModelError(nameof(viewModel.Message), $"Message text is too long. Maximum length is {Defaults.MaxMessageTextLength} characters.");
                return View("Create", viewModel);
            }

            if (threadCreateRm.MessageHtml.Length > Defaults.MaxMessageHtmlLength)
            {
                ModelState.AddModelError(nameof(viewModel.Message), $"Resulting HTML is too long. Maximum length is {Defaults.MaxMessageHtmlLength} characters.");
                return View("Create", viewModel);
            }

            var threadCreateResult = await _threadService.CreateThreadAsync(threadCreateRm, viewModel.Attachments, cancellationToken);

            var actionResult = threadCreateResult.Match<IActionResult>(
                success =>
                {
                    _logger.LogDebug(
                        LogEventIds.ThreadCreated,
                        "Thread created. ThreadId: {ThreadId}, PostId: {PostId}, CategoryAlias: {CategoryAlias}",
                        success.ThreadId,
                        success.PostId,
                        viewModel.CategoryAlias);

                    return RedirectToRoute(
                        "ThreadDetails",
                        new
                        {
                            categoryAlias = viewModel.CategoryAlias,
                            threadId = success.ThreadId,
                        });
                },
                err =>
                {
                    _logger.LogError(
                        LogEventIds.ThreadCreateError,
                        "Error creating thread. CategoryAlias: {CategoryAlias}, Error: {Error}",
                        viewModel.CategoryAlias,
                        err.ErrorMessage);

                    ViewBag.ErrorMessage = err.ErrorMessage;
                    return View("Create", viewModel);
                });

            return actionResult;
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
            return View("Create", viewModel);
        }
    }
}
