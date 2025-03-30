using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hikkaba.Infrastructure.Repositories.Contracts;

public interface ISeedManager
{
    Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userMgr,
        RoleManager<ApplicationRole> roleMgr,
        IOptions<SeedConfiguration> settings);
}
