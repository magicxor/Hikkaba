using System.Globalization;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class SeedManager : ISeedManager
{
    private readonly TimeProvider _timeProvider;

    public SeedManager(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    private async Task SeedNewCategoryAsync(
        ApplicationDbContext context,
        ApplicationUser createdBy,
        Board board,
        string alias,
        string name,
        bool isHidden = false,
        bool defaultShowThreadLocalUserHash = false,
        int defaultBumpLimit = Defaults.DefaultBumpLimit)
    {
        await context.Categories.AddAsync(new Category
        {
            Alias = alias,
            Name = name,
            IsHidden = isHidden,
            DefaultShowThreadLocalUserHash = defaultShowThreadLocalUserHash,
            DefaultBumpLimit = defaultBumpLimit,
            Board = board,
            CreatedBy = createdBy,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
        });
    }

    private async Task<ApplicationUser?> FindAdminUserAsync(
        UserManager<ApplicationUser> userMgr,
        string userName,
        string email)
    {
        var adminUser = await userMgr.FindByNameAsync(userName)
                        ?? await userMgr.FindByEmailAsync(email);
        return adminUser;
    }

    private async Task<ApplicationUser> GetOrCreateAdminUserAsync(
        UserManager<ApplicationUser> userMgr,
        SeedConfiguration seedConfiguration)
    {
        var adminUser = await FindAdminUserAsync(userMgr, Defaults.AdministratorUserName, seedConfiguration.AdministratorEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = Defaults.AdministratorUserName,
                Email = seedConfiguration.AdministratorEmail,
                EmailConfirmed = true,
                PhoneNumber = "88005553535",
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture),
                CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
                LockoutEnabled = false,
                TwoFactorEnabled = false,
            };
            var userCreateResult = await userMgr.CreateAsync(adminUser, seedConfiguration.AdministratorPassword);
            if (!userCreateResult.Succeeded)
            {
                throw new HikkabaDataException($"Can't create user {Defaults.AdministratorUserName}: {string.Join(", ",
                    userCreateResult.Errors.Select(e => $"{e.Code}:{e.Description}"))}");
            }
        }

        return adminUser;
    }

    private async Task<ApplicationRole> GetOrCreateAdminRoleAsync(RoleManager<ApplicationRole> roleMgr)
    {
        var adminRole = await roleMgr.FindByNameAsync(Defaults.AdministratorRoleName);

        if (adminRole == null)
        {
            adminRole = new ApplicationRole { Name = Defaults.AdministratorRoleName };
            var roleCreateResult = await roleMgr.CreateAsync(adminRole);
            if (!roleCreateResult.Succeeded)
            {
                throw new HikkabaDataException($"Can't create role {Defaults.AdministratorRoleName}: {string.Join(", ", roleCreateResult.Errors.Select(e => $"{e.Code}:{e.Description}"))}");
            }
        }

        return adminRole;
    }

    public async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userMgr,
        RoleManager<ApplicationRole> roleMgr,
        IOptions<SeedConfiguration> settings)
    {
        var seedConfiguration = settings.Value;

        var adminUser = await GetOrCreateAdminUserAsync(userMgr, seedConfiguration);
        var adminRole = await GetOrCreateAdminRoleAsync(roleMgr);

        var hasAdminRole = await userMgr.IsInRoleAsync(adminUser, adminRole.Name ?? Defaults.AdministratorRoleName);
        if (!hasAdminRole)
        {
            await userMgr.AddToRoleAsync(adminUser, Defaults.AdministratorRoleName);
        }

        Board board;
        if (!context.Boards.Any())
        {
            board = new Board {Name = Defaults.BoardName};
            await context.Boards.AddAsync(board);
        }
        else
        {
            board = await context.Boards.FirstAsync();
        }

        if (!context.Categories.Any())
        {
            await SeedNewCategoryAsync(context, adminUser, board, "a", "Anime");
            await SeedNewCategoryAsync(context, adminUser, board, "b", "Random");
            await SeedNewCategoryAsync(context, adminUser, board, "mu", "Music");
            await SeedNewCategoryAsync(context, adminUser, board, "nsfw", "18+ content", true);
            await SeedNewCategoryAsync(context, adminUser, board, "vg", "Video Games");
            await SeedNewCategoryAsync(context, adminUser, board, "wp", "Wallpapers");
            await SeedNewCategoryAsync(context, adminUser, board, "d", "Discussions about " + Defaults.BoardName, false, true);
        }

        await context.SaveChangesAsync();
    }
}
