using TPrimaryKey = System.Guid;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services;

public interface ICategoryToModeratorService
{
    Task<bool> IsUserCategoryModeratorAsync(TPrimaryKey categoryId, ClaimsPrincipal user);
    Task<IDictionary<CategoryDto, IList<ApplicationUserDto>>> ListCategoriesModeratorsAsync();
    Task<IDictionary<ApplicationUserDto, IList<CategoryDto>>> ListModeratorsCategoriesAsync();
    Task AddAsync(TPrimaryKey categoryId, TPrimaryKey moderatorId);
    Task DeleteAsync(TPrimaryKey categoryId, TPrimaryKey moderatorId);
}

public class CategoryToModeratorService : ICategoryToModeratorService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public CategoryToModeratorService(ApplicationDbContext context,
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
    }
        
    public async Task<IDictionary<CategoryDto, IList<ApplicationUserDto>>> ListCategoriesModeratorsAsync()
    {
        var categoriesModeratorsEntityList = await _context.Categories
            .OrderBy(category => category.Alias)
            .Select(category => new
            {
                Category = category,
                Moderators = category.Moderators.Select(cm => cm.ApplicationUser).OrderBy(u => u.UserName).ToList(),
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
        var moderatorsCategoriesEntityList = await _context.Users
            .OrderBy(user => user.UserName)
            .Select(user => new
            {
                Moderator = user,
                Categories = user.ModerationCategories.Select(cm => cm.Category).OrderBy(c => c.Alias).ToList(),
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
        if (user != null && user.Identity.IsAuthenticated)
        {
            if (user.IsInRole(Defaults.AdministratorRoleName))
            {
                return true;
            }
            else
            {
                var userId = TPrimaryKey.Parse(_userManager.GetUserId(user));
                return await _context.CategoriesToModerators
                    .AnyAsync(cm => cm.CategoryId == categoryId && cm.ApplicationUserId == userId);
            }
        }
        else
        {
            return false;
        }
    }
        
    public async Task AddAsync(TPrimaryKey categoryId, TPrimaryKey moderatorId)
    {
        var entity = new CategoryToModerator
        {
            CategoryId = categoryId,
            ApplicationUserId = moderatorId,
        };
        await _context.CategoriesToModerators.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
        
    public async Task DeleteAsync(TPrimaryKey categoryId, TPrimaryKey moderatorId)
    {
        _context.CategoriesToModerators.RemoveRange(
            _context.CategoriesToModerators.Where(cm => 
                cm.CategoryId == categoryId 
                && cm.ApplicationUserId == moderatorId));
        await _context.SaveChangesAsync();
    }
}