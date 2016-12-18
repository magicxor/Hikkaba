using System;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Configuration;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hikkaba.Common.Data
{
    public static class DbSeeder
    {
        private const int DefaultBumpLimit = 500;

        private static async Task SeedNewCategoryAsync(ApplicationDbContext context, Board board, string alias, string name, bool isHidden = false, bool defaultShowThreadLocalUserHash = false, int defaultBumpLimit = DefaultBumpLimit, string creatorUserName = Defaults.DefaultAdminUserName)
        {
            await context.Categories.AddAsync(new Category()
            {
                Alias = alias,
                Name = name,
                IsHidden = isHidden,
                DefaultShowThreadLocalUserHash = defaultShowThreadLocalUserHash,
                DefaultBumpLimit = defaultBumpLimit,
                Board = board,
                CreatedBy = await context.Users.FirstOrDefaultAsync(u => u.UserName == creatorUserName),
            });
        }

        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userMgr, RoleManager<ApplicationRole> roleMgr, IOptions<SeedConfiguration> settings)
        {
            var seedConfiguration = settings.Value;
            
            if (!context.Roles.Any())
            {
                // create admin role
                var adminRole = await roleMgr.FindByNameAsync(Defaults.DefaultAdminRoleName);
                if (adminRole == null)
                {
                    adminRole = new ApplicationRole() { Name = Defaults.DefaultAdminRoleName };
                    await roleMgr.CreateAsync(adminRole);
                }
            }

            if (!context.Users.Any())
            {
                // create anonymous user
                var anonymousUser = new ApplicationUser()
                {
                    UserName = Defaults.DefaultAnonymousUserName,
                    Email = Defaults.DefaultAnonymousEmail
                };
                await userMgr.CreateAsync(anonymousUser, Defaults.DefaultAnonymousPassword);
                await userMgr.SetLockoutEnabledAsync(anonymousUser, true);

                // create admin user
                var adminUser = new ApplicationUser
                {
                    UserName = Defaults.DefaultAdminUserName,
                    Email = seedConfiguration.AdministratorEmail
                };
                await userMgr.CreateAsync(adminUser, seedConfiguration.AdministratorPassword);
                await userMgr.SetLockoutEnabledAsync(adminUser, true);
                await userMgr.AddToRoleAsync(adminUser, Defaults.DefaultAdminRoleName);
            }

            Board board;

            if (!context.Boards.Any())
            {
                board = new Board() {Name = Defaults.BoardName};
                await context.Boards.AddAsync(board);
            }
            else
            {
                board = await context.Boards.FirstAsync();
            }

            if (!context.Categories.Any())
            {
                await SeedNewCategoryAsync(context, board, "a", "Anime");
                await SeedNewCategoryAsync(context, board, "b", "Random");
                await SeedNewCategoryAsync(context, board, "mu", "Music");
                await SeedNewCategoryAsync(context, board, "nsfw", "18+ content", true);
                await SeedNewCategoryAsync(context, board, "vg", "Video Games");
                await SeedNewCategoryAsync(context, board, "wp", "Wallpapers");
                await SeedNewCategoryAsync(context, board, "d", "Discussions about " + Defaults.BoardName, false, true);
            }

            await context.SaveChangesAsync();
        }
    }
}