using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Base.Current;
using Microsoft.EntityFrameworkCore;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Services
{
    public interface ICategoryService : IBaseMutableEntityService<CategoryDto, Category>
    {
        Task<CategoryDto> GetAsync(string alias);
        Task<TPrimaryKey> CreateAsync(CategoryDto dto, TPrimaryKey currentUserId);
        Task EditAsync(CategoryDto dto, TPrimaryKey currentUserId);
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
        
        public async Task<CategoryDto> GetAsync(string alias)
        {
            var result = await GetAsync(entity => entity.Alias == alias);
            return result;
        }

        public async Task<TPrimaryKey> CreateAsync(CategoryDto dto, TPrimaryKey currentUserId)
        {
            var id = await base.CreateAsync(dto, currentUserId, category =>
            {
                category.Board = Context.Boards.FirstOrDefault();
            });
            return id;
        }

        public async Task EditAsync(CategoryDto dto, TPrimaryKey currentUserId)
        {
            await base.EditAsync(dto, currentUserId, category => 
            {
                category.Board = Context.Boards.FirstOrDefault();
            });
        }
    }
}
