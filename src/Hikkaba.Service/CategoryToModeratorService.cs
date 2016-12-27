using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Hikkaba.Service
{
    public interface ICategoryToModeratorService : IBaseManyToManyService<Guid, Guid>
    {
        Task<IDictionary<CategoryDto, IList<ApplicationUserDto>>> ListCategoriesModerators();
        Task<IDictionary<ApplicationUserDto, IList<CategoryDto>>> ListModeratorsCategories();
    }

    public class CategoryToModeratorService : BaseManyToManyService<CategoryToModerator, Guid, Guid>, ICategoryToModeratorService
    {
        private readonly IMapper _mapper;

        public CategoryToModeratorService(ApplicationDbContext context,
            IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        protected override DbSet<CategoryToModerator> GetManyToManyDbSet(ApplicationDbContext context)
        {
            return Context.CategoriesToModerators;
        }

        protected override CategoryToModerator CreateManyToManyEntity(Guid leftId, Guid rightId)
        {
            return new CategoryToModerator() { CategoryId = leftId, ApplicationUserId = rightId };
        }

        protected override Guid GetLeftEntityKey(CategoryToModerator manyToManyEntity)
        {
            return manyToManyEntity.CategoryId;
        }

        protected override Guid GetRightEntityKey(CategoryToModerator manyToManyEntity)
        {
            return manyToManyEntity.ApplicationUserId;
        }

        public async Task<IDictionary<CategoryDto, IList<ApplicationUserDto>>> ListCategoriesModerators()
        {
            var categoriesModeratorsEntity = await Context.Categories
                .OrderBy(category => category.Alias)
                .Select(category => new KeyValuePair<Category, IEnumerable<ApplicationUser>>(
                        category, category.Moderators.Select(cm => cm.ApplicationUser).OrderBy(u => u.UserName)
                    ))
                .ToListAsync();
            var categoriesModeratorsDto = new Dictionary<CategoryDto, IList<ApplicationUserDto>>();
            foreach (var categoryModerators in categoriesModeratorsEntity)
            {
                var categoryDto = _mapper.Map<CategoryDto>(categoryModerators.Key);
                var moderatorsDto = _mapper.Map<IList<ApplicationUserDto>>(categoryModerators.Value);
                categoriesModeratorsDto.Add(categoryDto, moderatorsDto);
            }
            return categoriesModeratorsDto;
        }

        public async Task<IDictionary<ApplicationUserDto, IList<CategoryDto>>> ListModeratorsCategories()
        {
            var moderatorsCategoriesEntity = await Context.Users
                .OrderBy(user => user.UserName)
                .Select(user => new KeyValuePair<ApplicationUser, IEnumerable<Category>>(
                        user, user.ModerationCategories.Select(mc => mc.Category).OrderBy(u => u.Alias)
                    ))
                .ToListAsync();
            var moderatorsCategoriesDto = new Dictionary<ApplicationUserDto, IList<CategoryDto>>();
            foreach (var moderatorCategories in moderatorsCategoriesDto)
            {
                var moderatorDto = _mapper.Map<ApplicationUserDto>(moderatorCategories.Key);
                var categoriesDto = _mapper.Map<IList<CategoryDto>>(moderatorCategories.Value);
                moderatorsCategoriesDto.Add(moderatorDto, categoriesDto);
            }
            return moderatorsCategoriesDto;
        }
    }
}
