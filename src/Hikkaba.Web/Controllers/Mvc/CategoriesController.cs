using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Service;
using Hikkaba.Service.Base;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Filters;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Hikkaba.Web.ViewModels.ThreadsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.Controllers.Mvc
{
    [TypeFilter(typeof(ExceptionLoggingFilter))]
    [Authorize]
    public class CategoriesController : BaseMvcController
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;

        public CategoriesController(
            UserManager<ApplicationUser> userManager, 
            ILogger<CategoriesController> logger,
            IMapper mapper,
            ICategoryService categoryService, 
            IThreadService threadService, 
            IPostService postService) : base(userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _categoryService = categoryService;
            _threadService = threadService;
            _postService = postService;
        }
        
        [AllowAnonymous]
        [Route("{categoryAlias}")]
        public async Task<IActionResult> Details(string categoryAlias, int page = 1, int size = 10)
        {
            var pageDto = new PageDto(page, size);
            var categoryDto = await _categoryService.GetAsync(categoryAlias);
            var threadDtoList = await _threadService.PagedListCategoryThreadsOrdered(categoryDto.Id, pageDto);

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

        [Route("Categories/Create")]
        public async Task<IActionResult> Create()
        {
            throw new NotImplementedException();
        }

        [Route("Categories/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryEditViewModel categoryEditViewModel)
        {
            throw new NotImplementedException();
        }

        [Route("Categories/{categoryId}/Edit")]
        public async Task<IActionResult> Edit(Guid categoryId)
        {
            throw new NotImplementedException();
        }

        [Route("Categories/{categoryId}/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid categoryId, CategoryEditViewModel categoryEditViewModel)
        {
            throw new NotImplementedException();
        }

        [Route("Categories/{categoryId}/Delete")]
        public async Task<IActionResult> Delete(Guid categoryId)
        {
            throw new NotImplementedException();
        }

        [Route("Categories/{categoryId}/Delete")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid categoryId)
        {
            throw new NotImplementedException();
        }
    }
}