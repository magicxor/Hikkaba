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
using Hikkaba.Service.Base;
using Hikkaba.Web.Controllers.Mvc.Base;
using Hikkaba.Web.Filters;
using Hikkaba.Web.ViewModels;
using Hikkaba.Web.ViewModels.CategoriesViewModels;
using Hikkaba.Web.ViewModels.HomeViewModels;
using Hikkaba.Web.ViewModels.PostsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc
{
    [TypeFilter(typeof(ExceptionLoggingFilter))]
    [Authorize]
    public class PostsController : BaseMvcController
    {
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;
        
        public PostsController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ICategoryService categoryService,
            IThreadService threadService,
            IPostService postService) : base(userManager)
        {
            _mapper = mapper;
            _categoryService = categoryService;
            _threadService = threadService;
            _postService = postService;
        }

        [Route("{categoryAlias}/Threads/{threadId}/Posts/Create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create(string categoryAlias, Guid threadId)
        {
            var category = await _categoryService.GetAsync(categoryAlias);
            var thread = await _threadService.GetAsync(threadId);
            var postAnonymousCreateViewModel = new PostAnonymousCreateViewModel()
            {
                CategoryAlias = category.Alias,
                ThreadId = thread.Id,
            };
            return View(postAnonymousCreateViewModel);
        }

        [Route("{categoryAlias}/Threads/{threadId}/Posts/Create")]
        [HttpPost]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.",
                    IsNumericErrorMessage = "The input value should be a number.",
                    CaptchaGeneratorLanguage = Language.English)]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create(string categoryAlias, Guid threadId, PostAnonymousCreateViewModel postAnonymousCreateViewModel)
        {
            if (ModelState.IsValid)
            {
                var thread = await _threadService.GetAsync(postAnonymousCreateViewModel.ThreadId);
                if (!thread.IsClosed)
                {
                    var postDto = _mapper.Map<PostDto>(postAnonymousCreateViewModel);
                    postDto.UserIpAddress = UserIpAddress.ToString();
                    postDto.UserAgent = UserAgent;

                    var postId = await _postService.CreateAsync(postAnonymousCreateViewModel.Attachments, postDto);
                    return
                        Redirect(Url.Action("Details", "Threads", 
                        new
                        {
                            categoryAlias = categoryAlias,
                            threadId = threadId
                        }) + "#" + postId);
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.Forbidden, $"Thread {thread.Id} is closed.");
                }
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
        }

        [Route("Search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string query, int page = 1, int size = 10)
        {
            var pageDto = new PageDto(page, size);

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, "query is empty");
            }
            else if (query.Trim().Length < 3)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, "query is too short");
            }
            else
            {
                var latestPostsDtoList = await _postService
                                           .PagedListAsync(
                                               post => 
                                                    (!post.IsDeleted) && 
                                                        ((post.Message.Contains(query)) ||
                                                            (post.Thread.Title.Contains(query) && 
                                                            (post == post.Thread.Posts.OrderBy(tp => tp.Created).FirstOrDefault()))
                                                        ),
                                               post => post.Created,
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

                var searchResultViewModel = new SearchResultViewModel()
                {
                    Query = query,
                    Posts = new BasePagedList<PostDetailsViewModel>()
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
        public async Task<IActionResult> Edit(string categoryAlias, Guid threadId, Guid postId)
        {
            throw new NotImplementedException();
        }

        [Route("{categoryAlias}/Threads/{threadId}/Posts/{postId}/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string categoryAlias, Guid threadId, Guid postId, PostEditViewModel postEditViewModel)
        {
            throw new NotImplementedException();
        }

        [Route("{categoryAlias}/Threads/{threadId}/Posts/{postId}/Delete")]
        public async Task<IActionResult> Delete(string categoryAlias, Guid threadId, Guid postId)
        {
            throw new NotImplementedException();
        }

        [Route("{categoryAlias}/Threads/{threadId}/Posts/{postId}/Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string categoryAlias, Guid threadId, Guid postId, PostEditViewModel postEditViewModel)
        {
            throw new NotImplementedException();
        }
    }
}