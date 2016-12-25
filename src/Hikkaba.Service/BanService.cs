using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Utils;
using Hikkaba.Service.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service
{
    public interface IBanService : IBaseModeratedMutableEntityService<BanDto, Ban, Guid>
    {
        bool IsInRange(string ipAddress, string rangeLowerIpAddress, string rangeUpperIpAddress);
        Task<Tuple<bool, string>> IsPostingAllowedAsync(Guid threadId, string userIpAddress);
    }

    public class BanService : BaseModeratedMutableEntityService<BanDto, Ban, Guid>, IBanService
    {
        private readonly ICategoryToModeratorService _categoryToModeratorService;

        public BanService(IMapper mapper, 
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ICategoryToModeratorService categoryToModeratorService) : base(mapper, context, userManager)
        {
            _categoryToModeratorService = categoryToModeratorService;
        }

        protected override Guid GetCategoryId(Ban entity)
        {
            return entity.Category.Id;
        }

        protected override IBaseManyToManyService<Guid, Guid> GetManyToManyService()
        {
            return _categoryToModeratorService;
        }

        protected override DbSet<Ban> GetDbSet(ApplicationDbContext context)
        {
            return context.Bans;
        }

        protected override IQueryable<Ban> GetDbSetWithReferences(ApplicationDbContext context)
        {
            return context.Bans
                .Include(ban => ban.RelatedPost)
                .Include(ban => ban.Category);
        }

        protected override void LoadReferenceFields(ApplicationDbContext context, Ban entityEntry)
        {
            context.Entry(entityEntry).Reference(x => x.RelatedPost).Load();
            context.Entry(entityEntry).Reference(x => x.Category).Load();
        }

        public bool IsInRange(string ipAddress, string rangeLowerIpAddress, string rangeUpperIpAddress)
        {
            var rangeStart = IPAddress.Parse(rangeLowerIpAddress);
            var rangeEnd = IPAddress.Parse(rangeUpperIpAddress);
            var range = new IPAddressRange(rangeStart, rangeEnd);
            return range.Contains(IPAddress.Parse(ipAddress));
        }

        public async Task<Tuple<bool, string>> IsPostingAllowedAsync(Guid threadId, string userIpAddress)
        {
            var bans = await Context
                .Bans
                .Include(ban => ban.Category)
                    .ThenInclude(category => category.Threads)
                .Where(
                    ban =>
                        ((ban.Category == null) || (ban.Category.Threads.Any(thread => thread.Id == threadId)))
                        && (!ban.IsDeleted)
                        && (ban.Start <= DateTime.Now)
                        && (ban.End >= DateTime.Now))
                .ToListAsync();

            var relatedBan = bans
                .FirstOrDefault(ban =>
                    IsInRange(userIpAddress, ban.LowerIpAddress, ban.UpperIpAddress));

            var isPostingAllowed = relatedBan == null;

            return new Tuple<bool, string>(isPostingAllowed, relatedBan?.Reason);
        }
    }
}