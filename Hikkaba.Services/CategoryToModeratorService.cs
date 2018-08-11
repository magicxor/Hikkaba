using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Base.Current;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Services
{
    public interface ICategoryToModeratorService : IBaseManyToManyService
    {
        Task<bool> IsUserCategoryModeratorAsync(TPrimaryKey categoryId, ClaimsPrincipal user);
        Task<IDictionary<CategoryDto, IList<ApplicationUserDto>>> ListCategoriesModeratorsAsync();
        Task<IDictionary<ApplicationUserDto, IList<CategoryDto>>> ListModeratorsCategoriesAsync();
    }

    public class CategoryToModeratorService : BaseManyToManyService<CategoryToModerator>, ICategoryToModeratorService
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

        protected override CategoryToModerator CreateManyToManyEntity(TPrimaryKey leftId, TPrimaryKey rightId)
        {
            return new CategoryToModerator { CategoryId = leftId, ApplicationUserId = rightId };
        }

        protected override TPrimaryKey GetLeftEntityKey(CategoryToModerator manyToManyEntity)
        {
            return manyToManyEntity.CategoryId;
        }

        protected override TPrimaryKey GetRightEntityKey(CategoryToModerator manyToManyEntity)
        {
            return manyToManyEntity.ApplicationUserId;
        }

        public async Task<IDictionary<CategoryDto, IList<ApplicationUserDto>>> ListCategoriesModeratorsAsync()
        {
            var categoriesModeratorsEntityList = await Context.Categories
                .OrderBy(category => category.Alias)
                .Select(category => new
                {
                    Category = category,
                    Moderators = category.Moderators.Select(cm => cm.ApplicationUser).OrderBy(u => u.UserName).ToList()
                })
                .ToListAsync();
            var categoriesModeratorsDtoList = new Dictionary<CategoryDto, IList<ApplicationUserDto>>();
            foreach (var categoryModerators in categoriesModeratorsEntityList)
            {
                var categoryDto = _mapper.Map<CategoryDto>(categoryModerators.Category);
                var moderatorsDto = _mapper.Map<IList<ApplicationUserDto>>(categoryModerators.Moderators);
                categoriesModeratorsDtoList.Add(categoryDto, moderatorsDto);
            }
            return categoriesModeratorsDtoList;
        }

        public async Task<IDictionary<ApplicationUserDto, IList<CategoryDto>>> ListModeratorsCategoriesAsync()
        {
            var moderatorsCategoriesEntityList = await Context.Users
                .OrderBy(user => user.UserName)
                .Select(user => new
                {
                    Moderator = user,
                    Categories = user.ModerationCategories.Select(mc => mc.Category).OrderBy(u => u.Alias).ToList()
                })
                .ToListAsync();
            var moderatorsCategoriesDtoList = new Dictionary<ApplicationUserDto, IList<CategoryDto>>();
            foreach (var moderatorCategories in moderatorsCategoriesEntityList)
            {
                var moderatorDto = _mapper.Map<ApplicationUserDto>(moderatorCategories.Moderator);
                var categoriesDto = _mapper.Map<IList<CategoryDto>>(moderatorCategories.Categories);
                moderatorsCategoriesDtoList.Add(moderatorDto, categoriesDto);
            }
            return moderatorsCategoriesDtoList;
        }

        public async Task<bool> IsUserCategoryModeratorAsync(TPrimaryKey categoryId, ClaimsPrincipal user)
        {
            if ((user != null) && user.Identity.IsAuthenticated)
            {
                if (user.IsInRole(Defaults.AdministratorRoleName))
                {
                    return true;
                }
                else
                {
                    if (user.Identity.IsAuthenticated)
                    {
                        var userId = TPrimaryKey.Parse(_userManager.GetUserId(user));
                        return await AreRelatedAsync(categoryId, userId);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
    }
}
