using System;
using System.Linq;
using System.Net;
using AutoMapper;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Utils;
using Hikkaba.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service
{
    public interface IBanService : IBaseMutableEntityService<BanDto, Ban, Guid>
    {
        bool IsInRange(string ipAddress, string rangeLowerIpAddress, string rangeUpperIpAddress);
    }

    public class BanService: BaseMutableEntityService<BanDto, Ban, Guid>, IBanService
    {
        public BanService(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
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
    }
}
