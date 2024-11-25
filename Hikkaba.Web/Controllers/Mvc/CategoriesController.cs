using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Models.Dto;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Exceptions;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hikkaba.Web.Utils;
using Hikkaba.Web.ViewModels.AdministrationViewModels;
using Hikkaba.Models.Enums;
using Hikkaba.Services.Contracts;
using Hikkaba.Services.Implementations.Generic;

namespace Hikkaba.Web.Controllers.Mvc;

[Authorize(Roles = Defaults.AdministratorRoleName)]
public class CategoriesController : BaseMvcController
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IBoardService _boardService;
    private readonly ICategoryService _categoryService;
    private readonly IThreadService _threadService;
    private readonly IPostService _postService;
    private readonly ICategoryToModeratorService _categoryToModeratorService;
    private readonly IApplicationUserService _applicationUserService;

    public CategoriesController(
        UserManager<ApplicationUser> userManager,
        ILogger<CategoriesController> logger,
        IMapper mapper,
        IBoardService boardService,
        ICategoryService categoryService,
        IThreadService threadService,
        IPostService postService,
        ICategoryToModeratorService categoryToModeratorService,
        IApplicationUserService applicationUserService) : base(userManager)
    {
        _logger = logger;
        _mapper = mapper;
        _boardService = boardService;
        _categoryService = categoryService;
        _threadService = threadService;
        _postService = postService;
        _categoryToModeratorService = categoryToModeratorService;
        _applicationUserService = applicationUserService;
    }

    [AllowAnonymous]
    [Route("{categoryAlias}")]
    public async Task<IActionResult> Details(string categoryAlias, int page = 1, int size = 10)
    {
        var pageDto = new PageDto(page, size);
        var categoryDto = await _categoryService.GetAsync(categoryAlias);
        var isCurrentUserCategoryModerator = await _categoryToModeratorService
            .IsUserCategoryModeratorAsync(categoryDto.Id, User);
        var threadDtoList = await _threadService.PagedListAsync(thread =>
                (isCurrentUserCategoryModerator || !thread.IsDeleted)
                && (isCurrentUserCategoryModerator || thread.Posts.Any(p => !p.IsDeleted))
                && (thread.Category.Id == categoryDto.Id),
            pageDto);

        if ((categoryDto.IsDeleted) && (!isCurrentUserCategoryModerator))
        {
            throw new HttpResponseException(HttpStatusCode.NotFound, $"Category {categoryDto.Alias} not found.");
        }

        var threadDetailsViewModels = _mapper.Map<IList<ThreadDetailsViewModel>>(threadDtoList.CurrentPageItems);
        foreach (var threadDetailsViewModel in threadDetailsViewModels)
        {
            var postsDtoListReversed = await _postService
                .PagedListAsync(
                    post => (!post.IsDeleted) && (post.Thread.Id == threadDetailsViewModel.Id),
                    post => post.Created,
                    AdditionalRecordType.Last,
                    true,
                    new PageDto(1, Defaults.LatestPostsCountOnCategoryPage));
            var postCount = postsDtoListReversed.TotalItemsCount;
            var lastPostsDtoList = postsDtoListReversed.CurrentPageItems.OrderBy(post => post.Created).ToList();
            var postDetailsViewModels = _mapper.Map<IList<PostDetailsViewModel>>(lastPostsDtoList);
            var i = 0;
            foreach (var latestPostDetailsViewModel in postDetailsViewModels)
            {
                if (i == 0)
                {
                    latestPostDetailsViewModel.Index = 0;
                }
                else
                {
                    latestPostDetailsViewModel.Index = postCount - (Defaults.LatestPostsCountOnCategoryPage - (i - 1));
                }
                latestPostDetailsViewModel.ThreadShowThreadLocalUserHash = threadDetailsViewModel.ShowThreadLocalUserHash;
                latestPostDetailsViewModel.CategoryAlias = categoryDto.Alias;
                latestPostDetailsViewModel.CategoryId = categoryDto.Id;
                i++;
            }

            threadDetailsViewModel.Posts = postDetailsViewModels;
            threadDetailsViewModel.CategoryAlias = categoryDto.Alias;
            threadDetailsViewModel.CategoryName = categoryDto.Name;
            threadDetailsViewModel.PostCount = postCount;
        }

        var categoryViewModel = _mapper.Map<CategoryDetailsViewModel>(categoryDto);
        var categoryDetailsViewModel = new CategoryThreadsViewModel
        {
            Category = categoryViewModel,
            Threads = new BasePagedList<ThreadDetailsViewModel>
            {
                TotalItemsCount = threadDtoList.TotalItemsCount,
                CurrentPage = pageDto,
                CurrentPageItems = threadDetailsViewModels,
            },
        };
        return View(categoryDetailsViewModel);
    }

    [Route("Categories/Create")]
    public IActionResult Create()
    {
        return View();
    }

    [Route("Categories/Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var board = await _boardService.GetBoardAsync();
            var dto = _mapper.Map<CategoryDto>(viewModel);
            dto.BoardId = board.Id;
            var id = await _categoryService.CreateAsync(dto);
            return RedirectToAction("Index", "Administration");
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }

    [Route("Categories/{id}/Edit")]
    public async Task<IActionResult> Edit(TPrimaryKey id)
    {
        var dto = await _categoryService.GetAsync(id);
        var viewModel = _mapper.Map<CategoryEditViewModel>(dto);
        return View(viewModel);
    }

    [Route("Categories/{id}/Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryEditViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var dto = await _categoryService.GetAsync(viewModel.Id);
            _mapper.Map(viewModel, dto);
            await _categoryService.EditAsync(dto);
            return RedirectToAction("Index", "Administration");
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetIsDeleted(TPrimaryKey id, bool isDeleted)
    {
        var categoryDto = await _categoryService.GetAsync(id);
        var isCurrentUserCategoryModerator = await _categoryToModeratorService
            .IsUserCategoryModeratorAsync(categoryDto.Id, User);
        if (isCurrentUserCategoryModerator)
        {
            await _categoryService.SetIsDeletedAsync(id, isDeleted);
            return RedirectToAction("Index", "Administration");
        }
        else
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden, $"Access denied");
        }
    }

    [Route("Categories/{id}/AddModerator")]
    public async Task<IActionResult> AddModerator(TPrimaryKey id)
    {
        var categoryDto = await _categoryService.GetAsync(id);
        var usersDto = await _applicationUserService.ListAsync(user => !user.IsDeleted
                                                                       && (user.ModerationCategories.All(c => c.CategoryId != id) || !user.ModerationCategories.Any()),
            user => user.NormalizedUserName);
        var categoryDetailsViewModel = _mapper.Map<CategoryDetailsViewModel>(categoryDto);
        var usersViewModel = _mapper.Map<IList<ApplicationUserViewModel>>(usersDto);
        var viewModel = new CategoryModeratorsViewModel
        {
            Category = categoryDetailsViewModel,
            Moderators = usersViewModel,
        };
        return View(viewModel);
    }

    [Route("Categories/{id}/AddModerator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddModeratorConfirmed(TPrimaryKey id, TPrimaryKey moderatorId)
    {
        await _categoryToModeratorService.AddAsync(id, moderatorId);
        return RedirectToAction("Index", "Administration");
    }

    [Route("Categories/{id}/RemoveModerator")]
    public async Task<IActionResult> RemoveModerator(TPrimaryKey id)
    {
        var categoryDto = await _categoryService.GetAsync(id);
        var usersDto = await _applicationUserService.ListAsync(u => u.ModerationCategories.Any(c => c.CategoryId == id),
            user => user.NormalizedUserName);
        var categoryDetailsViewModel = _mapper.Map<CategoryDetailsViewModel>(categoryDto);
        var usersViewModel = _mapper.Map<IList<ApplicationUserViewModel>>(usersDto);
        var viewModel = new CategoryModeratorsViewModel
        {
            Category = categoryDetailsViewModel,
            Moderators = usersViewModel,
        };
        return View(viewModel);
    }

    [Route("Categories/{id}/RemoveModerator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveModeratorConfirmed(TPrimaryKey id, TPrimaryKey moderatorId)
    {
        await _categoryToModeratorService.DeleteAsync(id, moderatorId);
        return RedirectToAction("Index", "Administration");
    }
}
