using TPrimaryKey = System.Guid;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Base.Current;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services
{
    public interface IApplicationUserService : IBaseImmutableEntityService<ApplicationUserDto, ApplicationUser>
    {
        Task<TPrimaryKey> CreateAsync(ApplicationUserDto dto);
        Task EditAsync(ApplicationUserDto dto);
    }

    public class ApplicationUserService : BaseImmutableEntityService<ApplicationUserDto, ApplicationUser>, IApplicationUserService
    {
        public ApplicationUserService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }

        protected override DbSet<ApplicationUser> GetDbSet(ApplicationDbContext context)
        {
            return context.Users;
        }

        public async Task<TPrimaryKey> CreateAsync(ApplicationUserDto dto)
        {
            return await base.CreateAsync(dto, user => {});
        }

        public async Task EditAsync(ApplicationUserDto dto)
        {
            await base.EditAsync(dto, user => { });
        }
    }
}