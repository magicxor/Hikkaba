using System.Globalization;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class SeedRepository : ISeedRepository
{
    private readonly ILogger<SeedRepository> _logger;
    private readonly SeedConfiguration _seedConfiguration;
    private readonly TimeProvider _timeProvider;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userMgr;
    private readonly RoleManager<ApplicationRole> _roleMgr;

    public SeedRepository(
        ILogger<SeedRepository> logger,
        IOptions<SeedConfiguration> settings,
        TimeProvider timeProvider,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userMgr,
        RoleManager<ApplicationRole> roleMgr)
    {
        _logger = logger;
        _seedConfiguration = settings.Value;
        _timeProvider = timeProvider;
        _context = context;
        _userMgr = userMgr;
        _roleMgr = roleMgr;
    }

    private async Task SeedNewCategoryAsync(
        ApplicationUser createdBy,
        Board board,
        string alias,
        string name,
        bool isHidden = false,
        bool showThreadLocalUserHash = false,
        int defaultBumpLimit = Defaults.DefaultBumpLimit,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding new category: {CategoryName}", name);

        await _context.Categories.AddAsync(new Category
        {
            Alias = alias,
            Name = name,
            IsHidden = isHidden,
            ShowThreadLocalUserHash = showThreadLocalUserHash,
            DefaultBumpLimit = defaultBumpLimit,
            Board = board,
            CreatedBy = createdBy,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            MaxThreadCount = Defaults.MaxThreadCountInCategory,
        }, cancellationToken);
    }

    private async Task<ApplicationUser?> FindAdminUserAsync(
        string userName,
        string email)
    {
        var adminUser = await _userMgr.FindByNameAsync(userName)
                        ?? await _userMgr.FindByEmailAsync(email);
        return adminUser;
    }

    private async Task<ApplicationUser> GetOrCreateAdminUserAsync()
    {
        var adminUser = await FindAdminUserAsync(Defaults.AdministratorUserName, _seedConfiguration.AdministratorEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = Defaults.AdministratorUserName,
                Email = _seedConfiguration.AdministratorEmail,
                EmailConfirmed = true,
                PhoneNumber = "88005553535",
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture),
                CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
                LockoutEnabled = true,
                TwoFactorEnabled = false,
            };
            var userCreateResult = await _userMgr.CreateAsync(adminUser, _seedConfiguration.AdministratorPassword);
            if (!userCreateResult.Succeeded)
            {
                throw new HikkabaDataException($"Can't create user {Defaults.AdministratorUserName}: {string.Join(", ",
                    userCreateResult.Errors.Select(e => $"{e.Code}:{e.Description}"))}");
            }
        }

        return adminUser;
    }

    private async Task<ApplicationRole> GetOrCreateAdminRoleAsync(string roleName)
    {
        var adminRole = await _roleMgr.FindByNameAsync(roleName);

        if (adminRole == null)
        {
            adminRole = new ApplicationRole { Name = roleName };
            var roleCreateResult = await _roleMgr.CreateAsync(adminRole);
            if (!roleCreateResult.Succeeded)
            {
                throw new HikkabaDataException($"Can't create role {roleName}: {string.Join(", ", roleCreateResult.Errors.Select(e => $"{e.Code}:{e.Description}"))}");
            }
        }

        return adminRole;
    }

    public async Task<bool> SeedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seeding database...");

        var adminUser = await GetOrCreateAdminUserAsync();
        var adminRole = await GetOrCreateAdminRoleAsync(Defaults.AdministratorRoleName);
        await GetOrCreateAdminRoleAsync(Defaults.ModeratorRoleName);

        var hasAdminRole = await _userMgr.IsInRoleAsync(adminUser, adminRole.Name ?? Defaults.AdministratorRoleName);
        if (!hasAdminRole)
        {
            await _userMgr.AddToRoleAsync(adminUser, Defaults.AdministratorRoleName);
        }

        Board board;
        if (!await _context.Boards.AnyAsync(cancellationToken))
        {
            board = new Board { Name = Defaults.BoardName };
            await _context.Boards.AddAsync(board, cancellationToken);
        }
        else
        {
            board = await _context.Boards.OrderBy(x => x.Id).FirstAsync(cancellationToken);
        }

        if (!await _context.Categories.AnyAsync(cancellationToken))
        {
            await SeedNewCategoryAsync(adminUser, board, "a", "Anime", cancellationToken: cancellationToken);
            await SeedNewCategoryAsync(adminUser, board, "b", "Random", cancellationToken: cancellationToken);
            await SeedNewCategoryAsync(adminUser, board, "mu", "Music", cancellationToken: cancellationToken);
            await SeedNewCategoryAsync(adminUser, board, "nsfw", "18+ content", true, cancellationToken: cancellationToken);
            await SeedNewCategoryAsync(adminUser, board, "vg", "Video Games", cancellationToken: cancellationToken);
            await SeedNewCategoryAsync(adminUser, board, "wp", "Wallpapers", cancellationToken: cancellationToken);
            await SeedNewCategoryAsync(adminUser, board, "d", "Discussions about " + Defaults.BoardName, false, true, cancellationToken: cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
