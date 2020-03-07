using TPrimaryKey = System.Guid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Extensions;
using Hikkaba.Infrastructure.Exceptions;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Base.Generic;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Services
{
    public interface IBanService
    {
        Task<BanDto> GetAsync(TPrimaryKey id);
        
        Task<IList<BanDto>> ListAsync<TOrderKey>(
            Expression<Func<Ban, bool>> where = null, 
            Expression<Func<Ban, TOrderKey>> orderBy = null, 
            bool isDescending = false);

        Task<BasePagedList<BanDto>> PagedListAsync<TOrderKey>(
            Expression<Func<Ban, bool>> where = null,
            Expression<Func<Ban, TOrderKey>> orderBy = null, bool isDescending = false,
            PageDto page = null);
        
        Task<PostingPermissionDto> IsPostingAllowedAsync(TPrimaryKey threadId, string userIpAddress);
        
        Task<TPrimaryKey> GetOrCreateAsync(BanDto dto);
        
        Task EditAsync(BanDto dto);
        
        Task DeleteAsync(TPrimaryKey id);
    }

    public class BanService : BaseEntityService, IBanService
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IIpAddressCalculator _ipAddressCalculator;

        public BanService(IMapper mapper,
            ApplicationDbContext context,
            IIpAddressCalculator ipAddressCalculator) : base(mapper)
        {
            _mapper = mapper;
            _context = context;
            _ipAddressCalculator = ipAddressCalculator;
        }
        
        private IQueryable<Ban> Query<TOrderKey>(Expression<Func<Ban, bool>> where = null, Expression<Func<Ban, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = _context.Bans.AsQueryable();

            if (where != null)
            {
                query = query.Where(where);
            }

            if (orderBy != null)
            {
                if (isDescending)
                {
                    query = query.OrderByDescending(orderBy);
                }
                else
                {
                    query = query.OrderBy(orderBy);
                }
            }

            return query;
        }
        
        public async Task<BanDto> GetAsync(TPrimaryKey id)
        {
            var entity = await _context.Bans.FirstOrDefaultAsync(u => u.Id == id);
            var dto = MapEntityToDto<BanDto, Ban>(entity);
            return dto;
        }

        public async Task<IList<BanDto>> ListAsync<TOrderKey>(Expression<Func<Ban, bool>> where = null, Expression<Func<Ban, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = Query(where, orderBy, isDescending);
            var entityList = await query.ToListAsync();
            var dtoList = MapEntityListToDtoList<BanDto, Ban>(entityList);
            return dtoList;
        }

        public async Task<BasePagedList<BanDto>> PagedListAsync<TOrderKey>(Expression<Func<Ban, bool>> where = null, Expression<Func<Ban, TOrderKey>> orderBy = null, bool isDescending = false, PageDto page = null)
        {
            page = page ?? new PageDto();

            var query = Query(where, orderBy, isDescending);

            var pageQuery = query.Skip(page.Skip).Take(page.PageSize);

            var entityList = await pageQuery.ToListAsync();
            var dtoList = MapEntityListToDtoList<BanDto, Ban>(entityList);
            var pagedList = new BasePagedList<BanDto>
            {
                TotalItemsCount = query.Count(),
                CurrentPage = page,
                CurrentPageItems = dtoList,
            };
            return pagedList;
        }

        public async Task<TPrimaryKey> GetOrCreateAsync(BanDto dto)
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
                .FirstOrDefaultAsync(ban => (!ban.IsDeleted) && (ban.RelatedPost.Id == dto.RelatedPost.Id));
            }

            if (existingBan != null)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict, $"Ban already exists");
            }
            else
            {
                var entity = MapDtoToNewEntity<BanDto, Ban>(dto);
                entity.Category = dto.Category == null ? null : _context.GetLocalOrAttach<Category>(dto.Category.Id);
                entity.RelatedPost = dto.RelatedPost == null ? null : _context.GetLocalOrAttach<Post>(dto.RelatedPost.Id);
                await _context.Bans.AddAsync(entity);
                await _context.SaveChangesAsync();
                
                return entity.Id;
            }
        }
        
        public async Task EditAsync(BanDto dto)
        {
            var existingEntity = await _context.Bans.FirstOrDefaultAsync(ban => ban.Id == dto.Id);
            existingEntity.Category = dto.Category == null ? null : _context.GetLocalOrAttach<Category>(dto.Category.Id);
            existingEntity.RelatedPost = dto.RelatedPost == null ? null : _context.GetLocalOrAttach<Post>(dto.RelatedPost.Id);
            MapDtoToExistingEntity(dto, existingEntity);
            await _context.SaveChangesAsync();
        }
        
        public async Task<PostingPermissionDto> IsPostingAllowedAsync(TPrimaryKey threadId, string userIpAddress)
        {
            var bans = await _context.Bans
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

            BanDto banDto = null;
            if (!isPostingAllowed)
            {
                banDto = _mapper.Map<BanDto>(relatedBan);
            }

            return new PostingPermissionDto { IsPostingAllowed = isPostingAllowed, Ban = banDto };
        }
        
        public async Task DeleteAsync(TPrimaryKey id)
        {
            var entity = _context.GetLocalOrAttach<Ban>(id);
            entity.IsDeleted = true;
            _context.Entry(entity).Property(e => e.IsDeleted).IsModified = true;
            await _context.SaveChangesAsync();
        }
    }
}