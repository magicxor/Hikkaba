using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Mappings;
using Hikkaba.Infrastructure.Models.Administration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class AdministrationRepository : IAdministrationRepository
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
