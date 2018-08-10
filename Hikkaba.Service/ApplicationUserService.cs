using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service
{
    public interface IApplicationUserService : IBaseImmutableEntityService<ApplicationUserDto, ApplicationUser>
    {
        Task<Guid> CreateAsync(ApplicationUserDto dto);
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

        protected override IQueryable<ApplicationUser> GetDbSetWithReferences(ApplicationDbContext context)
        {
            return context.Users;
        }

        public async Task<Guid> CreateAsync(ApplicationUserDto dto)
        {
            return await base.CreateAsync(dto, user => {});
        }

        public async Task EditAsync(ApplicationUserDto dto)
        {
            await base.EditAsync(dto, user => { });
        }
    }
}