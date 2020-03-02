using TPrimaryKey = System.Guid;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Base.Current;
using Hikkaba.Services.Base.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services
{
    public interface IThreadService : IBaseModeratedMutableEntityService<ThreadDto, Thread>
    {
        Task<TPrimaryKey> CreateAsync(ThreadDto dto);
        Task EditAsync(ThreadDto dto, TPrimaryKey currentUserId);
        Task<BasePagedList<ThreadDto>> PagedListAsync(TPrimaryKey categoryId, PageDto page = null);
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

        protected override TPrimaryKey GetCategoryId(Thread entity)
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

        public async Task<TPrimaryKey> CreateAsync(ThreadDto dto)
        {
            return await base.CreateAsync(dto,
                thread =>
                {
                    thread.Category = Context.Categories.FirstOrDefault(category => category.Id == dto.CategoryId);
                });
        }

        public async Task EditAsync(ThreadDto dto, TPrimaryKey currentUserId)
        {
            await base.EditAsync(dto, currentUserId,
                thread =>
                {
                    thread.Category = Context.Categories.FirstOrDefault(category => category.Id == dto.CategoryId);
                });
        }

        public async Task<BasePagedList<ThreadDto>> PagedListAsync(TPrimaryKey categoryId, PageDto page = null)
        {
            page = page ?? new PageDto();

            var query = Context.Threads
                .Where(thread => (!thread.IsDeleted) && (thread.Posts.Any(p => !p.IsDeleted)) && (thread.Category.Id == categoryId))
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
            var pagedList = new BasePagedList<ThreadDto>
            {
                CurrentPage = page,
                TotalItemsCount = query.Count(),
                CurrentPageItems = dtoList
            };
            return pagedList;
        }
    }
}
