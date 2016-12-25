using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Exceptions;
using Hikkaba.Service.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service
{
    public interface IThreadService : IBaseModeratedMutableEntityService<ThreadDto, Thread, Guid>
    {
        Task<BasePagedList<ThreadDto>> PagedListCategoryThreadsOrdered(Guid categoryId, PageDto page = null);
    }

    public class ThreadService : BaseModeratedMutableEntityService<ThreadDto, Thread, Guid>, IThreadService
    {
        private readonly ICategoryToModeratorService _categoryToModeratorService;

        public ThreadService(IMapper mapper, 
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ICategoryToModeratorService categoryToModeratorService) : base(mapper, context, userManager)
        {
            _categoryToModeratorService = categoryToModeratorService;
        }

        protected override Guid GetCategoryId(Thread entity)
        {
            return entity.Category.Id;
        }

        protected override IBaseManyToManyService<Guid, Guid> GetManyToManyService()
        {
            return _categoryToModeratorService;
        }

        protected override DbSet<Thread> GetDbSet(ApplicationDbContext context)
        {
            return context.Threads;
        }

        protected override IQueryable<Thread> GetDbSetWithReferences(ApplicationDbContext context)
        {
            return context.Threads
                .Include(thread => thread.Category);
        }

        protected override void LoadReferenceFields(ApplicationDbContext context, Thread entityEntry)
        {
            context.Entry(entityEntry).Reference(thread => thread.Category).Load();
        }
        
        public async Task<BasePagedList<ThreadDto>> PagedListCategoryThreadsOrdered(Guid categoryId, PageDto page = null)
        {
            page = page ?? new PageDto();

            var query = Context.Threads
                .Include(thread => thread.Category)
                .Include(thread => thread.Posts)
                .Where(thread => (!thread.IsDeleted) && (thread.Category.Id == categoryId))
                .OrderByDescending(thread => thread.IsPinned)
                .ThenByDescending(thread => thread.Posts
                    .OrderBy(post => post.Created)
                    .Take(thread.BumpLimit).
                    OrderByDescending(post => post.Created)
                    .FirstOrDefault(post => (!post.IsSageEnabled) && (!post.IsDeleted))
                    .Created);

            var pagedQuery = query
                .Skip(page.Skip)
                .Take(page.PageSize);

            var entityList = await pagedQuery.ToListAsync();
            var dtoList = MapEntityListToDtoList(entityList);
            var pagedList = new BasePagedList<ThreadDto>()
            {
                CurrentPage = page,
                TotalItemsCount = query.Count(),
                CurrentPageItems = dtoList
            };
            return pagedList;
        }
    }
}
