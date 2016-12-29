using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Exceptions;
using Hikkaba.Service;
using Hikkaba.Service.Base;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Filters;
using Hikkaba.Web.ViewModels.BansViewModels;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc
{
    // todo: add details page for categories
    // todo: category moderators management

    [TypeFilter(typeof(ExceptionLoggingFilter))]
    [Authorize(Roles = Defaults.AdministratorRoleName)]
    public class CategoriesController : BaseMvcController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;
        private readonly ICategoryToModeratorService _categoryToModeratorService;

        public CategoriesController(
            UserManager<ApplicationUser> userManager, 
            ILogger<CategoriesController> logger,
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
        
        [AllowAnonymous]
        [Route("{categoryAlias}")]
        public async Task<IActionResult> Details(string categoryAlias, int page = 1, int size = 10)
        {
            var pageDto = new PageDto(page, size);
            var categoryDto = await _categoryService.GetAsync(categoryAlias);
            var threadDtoList = await _threadService.PagedListCategoryThreadsOrdered(categoryDto.Id, pageDto);

            var isCurrentUserCategoryModerator = await _categoryToModeratorService
                                                .IsUserCategoryModeratorAsync(categoryDto.Id, User);
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
                                                true,
                                                new PageDto(1, 3));
                var postCount = postsDtoListReversed.TotalItemsCount;
                var lastPostsDtoList = postsDtoListReversed.CurrentPageItems.OrderBy(post => post.Created).ToList();
                var postDetailsViewModels = _mapper.Map<IList<PostDetailsViewModel>>(lastPostsDtoList);
                foreach (var latestPostDetailsViewModel in postDetailsViewModels)
                {
                    latestPostDetailsViewModel.ThreadShowThreadLocalUserHash = threadDetailsViewModel.ShowThreadLocalUserHash;
                    latestPostDetailsViewModel.CategoryAlias = categoryDto.Alias;
                    latestPostDetailsViewModel.CategoryId = categoryDto.Id;
                }

                threadDetailsViewModel.Posts = postDetailsViewModels;
                threadDetailsViewModel.CategoryAlias = categoryDto.Alias;
                threadDetailsViewModel.CategoryName = categoryDto.Name;
                threadDetailsViewModel.PostCount = postCount;
            }
            
            var categoryViewModel = _mapper.Map<CategoryViewModel>(categoryDto);
            var categoryDetailsViewModel = new CategoryDetailsViewModel()
            {
                Category = categoryViewModel,
                Threads = new BasePagedList<ThreadDetailsViewModel>()
                {
                    TotalItemsCount = threadDtoList.TotalItemsCount,
                    CurrentPage = pageDto,
                    CurrentPageItems = threadDetailsViewModels,
                },
            };
            return View(categoryDetailsViewModel);
        }

        [Route("Categories")]
        public async Task<IActionResult> Index()
        {
            var dtoList = await _categoryService.ListAsync();
            var viewModelList = _mapper.Map<List<CategoryViewModel>>(dtoList);
            return View(viewModelList);
        }

        [Route("Categories/Create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [Route("Categories/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel categoryEditViewModel)
        {
            var dto = _mapper.Map<CategoryDto>(categoryEditViewModel);
            var id = await _categoryService.CreateAsync(dto, CurrentUserId);
            return RedirectToAction("Index");
        }

        [Route("Categories/{categoryId}/Edit")]
        public async Task<IActionResult> Edit(Guid categoryId)
        {
            var dto = await _categoryService.GetAsync(categoryId);
            var viewModel = _mapper.Map<CategoryViewModel>(dto);
            return View(viewModel);
        }

        [Route("Categories/{categoryId}/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel categoryViewModel)
        {
            var dto = _mapper.Map<CategoryDto>(categoryViewModel);
            await _categoryService.EditAsync(dto, CurrentUserId);
            return RedirectToAction("Index");
        }

        [Route("Categories/{categoryId}/Delete")]
        public async Task<IActionResult> Delete(Guid categoryId)
        {
            var dto = await _categoryService.GetAsync(categoryId);
            var viewModel = _mapper.Map<CategoryViewModel>(dto);
            return View(viewModel);
        }

        [Route("Categories/{categoryId}/Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid categoryId)
        {
            await _categoryService.DeleteAsync(categoryId, CurrentUserId);
            return RedirectToAction("Index");
        }
    }
}