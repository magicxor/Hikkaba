using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service
{
    public interface ICategoryService : IBaseMutableEntityService<CategoryDto, Category>
    {
        Task<CategoryDto> GetAsync(string alias);
        Task<Guid> CreateAsync(CategoryDto dto, Guid currentUserId);
        Task EditAsync(CategoryDto dto, Guid currentUserId);
    }

    public class CategoryService : BaseMutableEntityService<CategoryDto, Category>, ICategoryService
    {
        public CategoryService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        protected override DbSet<Category> GetDbSet(ApplicationDbContext context)
        {
            return context.Categories;
        }

        protected override IQueryable<Category> GetDbSetWithReferences(ApplicationDbContext context)
        {
            return context.Categories.Include(category => category.Board);
        }
        
        public async Task<CategoryDto> GetAsync(string alias)
        {
            var result = await GetAsync(entity => entity.Alias == alias);
            return result;
        }

        public async Task<Guid> CreateAsync(CategoryDto dto, Guid currentUserId)
        {
            var id = await base.CreateAsync(dto, currentUserId, category =>
            {
                category.Board = Context.Boards.FirstOrDefault();
            });
            return id;
        }

        public async Task EditAsync(CategoryDto dto, Guid currentUserId)
        {
            await base.EditAsync(dto, currentUserId, category => 
            {
                category.Board = Context.Boards.FirstOrDefault();
            });
        }
    }
}
