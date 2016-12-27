using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Hikkaba.Common.Constants;
using Microsoft.AspNetCore.Identity;

namespace Hikkaba.Service
{
    // todo: check ALL async method names in ALL services
    public interface ICategoryToModeratorService : IBaseManyToManyService<Guid, Guid>
    {
        Task<bool> IsUserCategoryModerator(Guid categoryId, ClaimsPrincipal user);
        Task<IDictionary<CategoryDto, IList<ApplicationUserDto>>> ListCategoriesModerators();
        Task<IDictionary<ApplicationUserDto, IList<CategoryDto>>> ListModeratorsCategories();
    }

    public class CategoryToModeratorService : BaseManyToManyService<CategoryToModerator, Guid, Guid>, ICategoryToModeratorService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryToModeratorService(ApplicationDbContext context,
            IMapper mapper,
            UserManager<ApplicationUser> userManager) : base(context)
        {
            _mapper = mapper;
            _userManager = userManager;
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

        public async Task<bool> IsUserCategoryModerator(Guid categoryId, ClaimsPrincipal user)
        {
            if ((user != null) && user.Identity.IsAuthenticated)
            {
                if (user.IsInRole(Defaults.DefaultAdminRoleName))
                {
                    return true;
                }
                else
                {
                    var userId = user.Identity.IsAuthenticated
                                    ? Guid.Parse(_userManager.GetUserId(user))
                                    : default(Guid);
                    return await AreRelatedAsync(categoryId, userId);
                }
            }
            else
            {
                return false;
            }
        }
    }
}
