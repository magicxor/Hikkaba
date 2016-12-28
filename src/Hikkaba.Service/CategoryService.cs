using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Exceptions;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service
{
    public interface ICategoryService : IBaseMutableEntityService<CategoryDto, Category, Guid>
    {
        Task<CategoryDto> GetAsync(string alias);
    }

    public class CategoryService : BaseMutableEntityService<CategoryDto, Category, Guid>, ICategoryService
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

        protected override void LoadReferenceFields(ApplicationDbContext context, Category entityEntry)
        {
            context.Entry(entityEntry).Reference(category => category.Board).Load();
        }

        protected async Task<Category> GetCategoryByAliasAsync(string alias)
        {
            var resultEntity = await GetDbSetWithReferences(Context).AsNoTracking().FirstOrDefaultAsync(entity => entity.Alias == alias);
            if (resultEntity == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound, $"{typeof(Category)} {alias} not found.");
            }
            else
            {
                return resultEntity;
            }
        }

        public async Task<CategoryDto> GetAsync(string alias)
        {
            var entity = await GetCategoryByAliasAsync(alias);
            return MapEntityToDto(entity);
        }
    }
}
