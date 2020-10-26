using System;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hikkaba.Data.Context
{
    public static class DbSeeder
    {
        private static async Task SeedNewCategoryAsync(ApplicationDbContext context, ApplicationUser createdBy, Board board, string alias, string name, bool isHidden = false, bool defaultShowThreadLocalUserHash = false, int defaultBumpLimit = Defaults.DefaultBumpLimit)
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
            });
        }

        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userMgr, RoleManager<ApplicationRole> roleMgr, IOptions<SeedConfiguration> settings)
        {
            var seedConfiguration = settings.Value;

            var adminRole = await roleMgr.FindByNameAsync(Defaults.AdministratorRoleName);
            if (adminRole == null)
            {
                // create admin role
                adminRole = new ApplicationRole { Name = Defaults.AdministratorRoleName };
                var roleCreateResult = await roleMgr.CreateAsync(adminRole);
                if (!roleCreateResult.Succeeded)
                {
                    throw new Exception($"Can't create role {Defaults.AdministratorRoleName}: {string.Join(", ", roleCreateResult.Errors.Select(e => $"{e.Code}:{e.Description}"))}");
                }
            }

            var adminUserToRole = await context.UserRoles.FirstOrDefaultAsync(ur => ur.RoleId == adminRole.Id);
            ApplicationUser adminUser;
            if (adminUserToRole == null)
            {
                // create admin user
                adminUser = new ApplicationUser
                {
                    UserName = Defaults.AdministratorUserName,
                    Email = seedConfiguration.AdministratorEmail,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                };
                var userCreateResult = await userMgr.CreateAsync(adminUser, seedConfiguration.AdministratorPassword);
                if (!userCreateResult.Succeeded)
                {
                    throw new Exception($"Can't create user {Defaults.AdministratorUserName}: {string.Join(", ", userCreateResult.Errors.Select(e => $"{e.Code}:{e.Description}"))}");
                }
                await userMgr.AddToRoleAsync(adminUser, Defaults.AdministratorRoleName);
            }
            else
            {
                adminUser = await userMgr.FindByIdAsync(adminUserToRole.UserId.ToString());
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
}
