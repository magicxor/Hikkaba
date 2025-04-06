using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Mappings;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public class AdministrationRepository : IAdministrationRepository
{
    private readonly ApplicationDbContext _context;

    public AdministrationRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var dashboardItems = await _context.Categories
            .Include(category => category.CreatedBy)
            .Include(category => category.ModifiedBy)
            .OrderBy(category => category.Alias)
            .Select(
                category =>
                    new
                    {
                        Category = category,
                        Moderators = category.Moderators
                            .OrderBy(categoryToModerator => categoryToModerator.Moderator.UserName)
                            .Select(categoryToModerator => categoryToModerator.Moderator)
                            .ToList(),
                    })
            .ToListAsync(cancellationToken);

        var dashboardItemsDto = dashboardItems
            .Select(categoryModerators => new CategoryModeratorsModel
            {
                Category = categoryModerators.Category.ToDashboardModel(),
                Moderators = categoryModerators.Moderators.ToPreviews(),
            })
            .ToList();

        return new DashboardModel
        {
            CategoriesModerators = dashboardItemsDto,
        };
    }
}
