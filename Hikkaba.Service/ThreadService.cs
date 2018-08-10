using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Service.Base;
using Hikkaba.Service.Base.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Service
{
    public interface IThreadService : IBaseModeratedMutableEntityService<ThreadDto, Thread>
    {
        Task<Guid> CreateAsync(ThreadDto dto);
        Task EditAsync(ThreadDto dto, Guid currentUserId);
        Task<BasePagedList<ThreadDto>> PagedListCategoryThreadsOrdered(Guid categoryId, PageDto page = null);
    }

    public class ThreadService : BaseModeratedMutableEntityService<ThreadDto, Thread>, IThreadService
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

        protected override IBaseManyToManyService<TPrimaryKey, TPrimaryKey> GetManyToManyService()
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

        public async Task<Guid> CreateAsync(ThreadDto dto)
        {
            return await base.CreateAsync(dto,
                thread =>
                {
                    thread.Category = Context.Categories.FirstOrDefault(category => category.Id == dto.CategoryId);
                });
        }

        public async Task EditAsync(ThreadDto dto, Guid currentUserId)
        {
            await base.EditAsync(dto, currentUserId,
                thread =>
                {
                    thread.Category = Context.Categories.FirstOrDefault(category => category.Id == dto.CategoryId);
                });
        }

        public async Task<BasePagedList<ThreadDto>> PagedListCategoryThreadsOrdered(Guid categoryId, PageDto page = null)
        {
            page = page ?? new PageDto();

            var query = Context.Threads.AsNoTracking()
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
