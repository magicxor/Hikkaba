using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Exceptions;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Base.Concrete;
using Hikkaba.Services.Base.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Services
{
    public interface IBanService : IBaseModeratedMutableEntityService<BanDto, Ban>
    {
        Task<Tuple<bool, string>> IsPostingAllowedAsync(TPrimaryKey threadId, string userIpAddress);
        Task<TPrimaryKey> CreateOrGetIdAsync(BanDto dto, TPrimaryKey currentUserId);
        Task EditAsync(BanDto dto, TPrimaryKey currentUserId);
    }

    public class BanService : BaseModeratedMutableEntityService<BanDto, Ban>, IBanService
    {
        private readonly ICategoryToModeratorService _categoryToModeratorService;
        private readonly IIpAddressCalculator _ipAddressCalculator;

        public BanService(IMapper mapper,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ICategoryToModeratorService categoryToModeratorService,
            IIpAddressCalculator ipAddressCalculator) : base(mapper, context, userManager)
        {
            _categoryToModeratorService = categoryToModeratorService;
            _ipAddressCalculator = ipAddressCalculator;
        }

        protected override TPrimaryKey GetCategoryId(Ban entity)
        {
            return entity.Category.Id;
        }

        protected override IBaseManyToManyService<TPrimaryKey, TPrimaryKey> GetManyToManyService()
        {
            return _categoryToModeratorService;
        }

        protected override DbSet<Ban> GetDbSet(ApplicationDbContext context)
        {
            return context.Bans;
        }

        public async Task<TPrimaryKey> CreateOrGetIdAsync(BanDto dto, TPrimaryKey currentUserId)
        {
            if (dto == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound, $"{nameof(dto)} is null");
            }

            // user can't be banned twice for one post, so we will return existing ban id in this case
            Ban existingBan = null;
            if (dto.RelatedPost != null)
            {
                existingBan = await Context.Bans
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
                    relatedPost = await Context.Posts.FirstOrDefaultAsync(post => post.Id == dto.RelatedPost.Id);
                    if (relatedPost == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound, $"Post {dto.RelatedPost.Id} not found");
                    }
                }

                Category relatedCategory = null;
                if (dto.Category != null)
                {
                    relatedCategory = await Context.Categories.FirstOrDefaultAsync(category => category.Id == dto.Category.Id);
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

        public async Task EditAsync(BanDto dto, TPrimaryKey currentUserId)
        {
            await base.EditAsync(dto, currentUserId, ban =>
            {
                ban.RelatedPost = dto.RelatedPost == null ? null : Context.Posts.FirstOrDefault(post => post.Id == dto.RelatedPost.Id);
                ban.Category = dto.Category == null ? null : Context.Categories.FirstOrDefault(category => category.Id == dto.Category.Id);
            });
        }

        public async Task<Tuple<bool, string>> IsPostingAllowedAsync(TPrimaryKey threadId, string userIpAddress)
        {
            var bans = await Context
                .Bans
                .Where(
                    ban =>
                        ((ban.Category == null) || (ban.Category.Threads.Any(thread => thread.Id == threadId)))
                        && (!ban.IsDeleted)
                        && (ban.Start <= DateTime.UtcNow)
                        && (ban.End >= DateTime.UtcNow))
                .ToListAsync();

            var relatedBan = bans
                .FirstOrDefault(ban =>
                    _ipAddressCalculator.IsInRange(userIpAddress, ban.LowerIpAddress, ban.UpperIpAddress));

            var isPostingAllowed = relatedBan == null;

            return new Tuple<bool, string>(isPostingAllowed, relatedBan?.Reason);
        }
    }
}