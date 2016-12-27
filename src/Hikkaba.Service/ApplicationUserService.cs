using System;
using System.Linq;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service
{
    public interface IApplicationUserService : IBaseImmutableEntityService<ApplicationUserDto, ApplicationUser> { }

    public class ApplicationUserService: BaseImmutableEntityService<ApplicationUserDto, ApplicationUser>, IApplicationUserService
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
            return context.Users
                .Include(user => user.CreatedBans)
                .Include(user => user.CreatedCategories)
                .Include(user => user.CreatedNotices)
                .Include(user => user.CreatedPosts)
                .Include(user => user.ModerationCategories)
                .Include(user => user.ModifiedBans)
                .Include(user => user.ModifiedCategories)
                .Include(user => user.ModifiedPosts);
        }

        protected override void LoadReferenceFields(ApplicationDbContext context, ApplicationUser entityEntry)
        {
            context.Entry(entityEntry).Collection(user => user.CreatedBans).Load();
            context.Entry(entityEntry).Collection(user => user.CreatedCategories).Load();
            context.Entry(entityEntry).Collection(user => user.CreatedNotices).Load();
            context.Entry(entityEntry).Collection(user => user.CreatedPosts).Load();
            context.Entry(entityEntry).Collection(user => user.ModerationCategories).Load();
            context.Entry(entityEntry).Collection(user => user.ModifiedBans).Load();
            context.Entry(entityEntry).Collection(user => user.ModifiedCategories).Load();
            context.Entry(entityEntry).Collection(user => user.ModifiedPosts).Load();
        }
    }
}