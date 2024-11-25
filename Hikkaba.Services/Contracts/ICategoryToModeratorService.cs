using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Hikkaba.Models.Dto;

namespace Hikkaba.Services.Contracts;

public interface ICategoryToModeratorService
{
    Task<bool> IsUserCategoryModeratorAsync(TPrimaryKey categoryId, ClaimsPrincipal user);
    Task<IDictionary<CategoryDto, IList<ApplicationUserDto>>> ListCategoriesModeratorsAsync();
    Task<IDictionary<ApplicationUserDto, IList<CategoryDto>>> ListModeratorsCategoriesAsync();
    Task AddAsync(TPrimaryKey categoryId, TPrimaryKey moderatorId);
    Task DeleteAsync(TPrimaryKey categoryId, TPrimaryKey moderatorId);
}