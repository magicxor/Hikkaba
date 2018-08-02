using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Exceptions;
using Hikkaba.Infrastructure.Utils;
using Hikkaba.Models.Dto;
using Hikkaba.Service.Base.Current;
using Hikkaba.Service.Base.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Service
{
    public interface IBanService : IBaseModeratedMutableEntityService<BanDto, Ban>
    {
        bool IsInRange(string ipAddress, string rangeLowerIpAddress, string rangeUpperIpAddress);
        Task<Tuple<bool, string>> IsPostingAllowedAsync(Guid threadId, string userIpAddress);
        Task<Guid> CreateOrGetIdAsync(BanDto dto, Guid currentUserId);
        Task EditAsync(BanDto dto, Guid currentUserId);
    }

    public class BanService : BaseModeratedMutableEntityService<BanDto, Ban>, IBanService
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ICategoryToModeratorService _categoryToModeratorService;

        public BanService(IMapper mapper,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ICategoryToModeratorService categoryToModeratorService) : base(mapper, context, userManager)
        {
            _mapper = mapper;
            _context = context;
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

        public bool IsInRange(string ipAddress, string rangeLowerIpAddress, string rangeUpperIpAddress)
        {
            var rangeStart = IPAddress.Parse(rangeLowerIpAddress);
            var rangeEnd = IPAddress.Parse(rangeUpperIpAddress);
            var range = new IPAddressRange(rangeStart, rangeEnd);
            return range.Contains(IPAddress.Parse(ipAddress));
        }

        public async Task<Guid> CreateOrGetIdAsync(BanDto dto, Guid currentUserId)
        {
            if (dto == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound, $"{nameof(dto)} is null");
            }

            // user can't be banned twice for one post, so we will return existing ban id in this case
            Ban existingBan = null;
            if (dto.RelatedPost != null)
            {
                existingBan = await _context.Bans
                .Include(ban => ban.Category)
                .Include(ban => ban.RelatedPost)
                .FirstOrDefaultAsync(ban => (!ban.IsDeleted) && (ban.RelatedPost.Id == dto.RelatedPost.Id));
            }

            if (existingBan != null)
            {
                return existingBan.Id;
            }
            else
            {
                Post relatedPost = null;
                if (dto.RelatedPost != null)
                {
                    relatedPost = await _context.Posts.FirstOrDefaultAsync(post => post.Id == dto.RelatedPost.Id);
                    if (relatedPost == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound, $"Post {dto.RelatedPost.Id} not found");
                    }
                }

                Category relatedCategory = null;
                if ((dto.Category != null) && (dto.Category?.Id != default(Guid)))
                {
                    relatedCategory = await _context.Categories.FirstOrDefaultAsync(category => category.Id == dto.Category.Id);
                    if (relatedCategory == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound, $"Category {dto.Category.Id} not found");
                    }
                }

                var banId = await base.CreateAsync(dto, currentUserId, (ban) =>
                {
                    ban.Category = relatedCategory;
                    ban.RelatedPost = relatedPost;
                });
                return banId;
            }
        }

        public async Task EditAsync(BanDto dto, Guid currentUserId)
        {
            await base.EditAsync(dto, currentUserId, ban =>
            {
                ban.RelatedPost = dto.RelatedPost == null ? null : Context.Posts.FirstOrDefault(post => post.Id == dto.RelatedPost.Id);
                ban.Category = dto.Category == null ? null : Context.Categories.FirstOrDefault(category => category.Id == dto.Category.Id);
            });
        }

        public async Task<Tuple<bool, string>> IsPostingAllowedAsync(Guid threadId, string userIpAddress)
        {
            var bans = await _context
                .Bans
                .Include(ban => ban.Category)
                    .ThenInclude(category => category.Threads)
                .Where(
                    ban =>
                        ((ban.Category == null) || (ban.Category.Threads.Any(thread => thread.Id == threadId)))
                        && (!ban.IsDeleted)
                        && (ban.Start <= DateTime.UtcNow)
                        && (ban.End >= DateTime.UtcNow))
                .ToListAsync();

            var relatedBan = bans
                .FirstOrDefault(ban =>
                    IsInRange(userIpAddress, ban.LowerIpAddress, ban.UpperIpAddress));

            var isPostingAllowed = relatedBan == null;

            return new Tuple<bool, string>(isPostingAllowed, relatedBan?.Reason);
        }
    }
}