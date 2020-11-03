using TPrimaryKey = System.Guid;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Models.Dto;
using Hikkaba.Services.Base.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Aggregations;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Data.Extensions;
using Hikkaba.Infrastructure.Exceptions;
using Hikkaba.Models.Configuration;
using Hikkaba.Models.Dto.Attachments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using TwentyTwenty.Storage;

namespace Hikkaba.Services
{
    public interface IThreadService
    {
        Task<ThreadDto> GetAsync(TPrimaryKey id);
        Task<ThreadAggregationDto> GetAggregationAsync(TPrimaryKey id, ClaimsPrincipal user);
        
        Task<IList<ThreadDto>> ListAsync<TOrderKey>(
            Expression<Func<Thread, bool>> where = null, 
            Expression<Func<Thread, TOrderKey>> orderBy = null, 
            bool isDescending = false);

        Task<ThreadPostCreateResultDto> CreateThreadPostAsync(IFormFileCollection attachments, ThreadPostCreateDto dto, bool createNewThread);
        
        Task EditAsync(ThreadDto dto);
        
        Task<BasePagedList<ThreadDto>> PagedListAsync(Expression<Func<Thread, bool>> where, PageDto page = null);
        
        Task SetIsPinnedAsync(TPrimaryKey id, bool newValue);
        Task SetIsClosedAsync(TPrimaryKey id, bool newValue);
        Task SetIsDeletedAsync(TPrimaryKey id, bool newValue);
    }

    public class ThreadService : BaseEntityService, IThreadService
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IStorageProvider _storageProvider;
        private readonly HikkabaConfiguration _hikkabaConfiguration;
        private readonly ICategoryToModeratorService _categoryToModeratorService;
        private readonly IBanService _banService;
        private readonly ICryptoService _cryptoService;
        private readonly IThumbnailGenerator _thumbnailGenerator;
        private readonly IAttachmentCategorizer _attachmentCategorizer;

        public ThreadService(IMapper mapper,
            ApplicationDbContext context,
            IStorageProvider storageProvider,
            IOptions<HikkabaConfiguration> settings,
            ICategoryToModeratorService categoryToModeratorService,
            IBanService banService,
            ICryptoService cryptoService,
            IThumbnailGenerator thumbnailGenerator,
            IAttachmentCategorizer attachmentCategorizer) : base(mapper)
        {
            _mapper = mapper;
            _context = context;
            _storageProvider = storageProvider;
            _hikkabaConfiguration = settings.Value;
            _categoryToModeratorService = categoryToModeratorService;
            _banService = banService;
            _cryptoService = cryptoService;
            _thumbnailGenerator = thumbnailGenerator;
            _attachmentCategorizer = attachmentCategorizer;
        }
        
        private IQueryable<Thread> Query<TOrderKey>(Expression<Func<Thread, bool>> where = null, Expression<Func<Thread, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = _context.Threads.AsQueryable();

            if (where != null)
            {
                query = query.Where(where);
            }

            if (orderBy != null)
            {
                query = isDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            }

            return query;
        }
        
        public async Task<ThreadDto> GetAsync(TPrimaryKey id)
        {
            var entity = await _context.Threads.FirstOrDefaultAsync(e => e.Id == id);
            var dto = MapEntityToDto<ThreadDto, Thread>(entity);
            return dto;
        }

        public async Task<ThreadAggregationDto> GetAggregationAsync(TPrimaryKey id, ClaimsPrincipal user)
        {
            var thread = await _context.Threads
                .Include(t => t.Category)
                .ThenInclude(c => c.Board)
                .FirstOrDefaultAsync(e => e.Id == id);
            
            var isUserCategoryModerator = await _categoryToModeratorService
                .IsUserCategoryModeratorAsync(thread.Category.Id, user);
            
            if ((thread.IsDeleted || thread.Category.IsDeleted) && (!isUserCategoryModerator))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound, $"Thread {id} not found.");
            }

            Expression<Func<Post, bool>> BuildPostFilterExpression()
            {
                if (isUserCategoryModerator)
                {
                    return post => post.Thread.Id == id;
                }
                else
                {
                    return post => post.Thread.Id == id && !post.IsDeleted;
                }
            }
            
            var posts = await _context.Posts
                .Include(e => e.Attachments)
                .Where(BuildPostFilterExpression())
                .OrderBy(post => post.Created)
                .ToListAsync();
            
            var threadPosts = new ThreadPosts
            {
                Thread = thread,
                Posts = posts,
            };
            
            var dto = _mapper.Map<ThreadAggregationDto>(threadPosts);
            return dto;
        }

        public async Task<IList<ThreadDto>> ListAsync<TOrderKey>(Expression<Func<Thread, bool>> where = null, Expression<Func<Thread, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = Query(where, orderBy, isDescending);
            var entityList = await query.ToListAsync();
            var dtoList = MapEntityListToDtoList<ThreadDto, Thread>(entityList);
            return dtoList;
        }

        private async Task CreateAttachmentsAsync(string containerName, Post postEntity, IFormFileCollection attachments)
        {
            foreach (var attachment in attachments)
            {
                var extension = Path.GetExtension(attachment.FileName)?.ToLowerInvariant()?.TrimStart('.');
                var fileName = Path.GetFileNameWithoutExtension(attachment.FileName);
                var fileNameWithExtension = fileName + "." + extension;
                var attachmentParentDto = _attachmentCategorizer.CreateAttachmentDto(fileNameWithExtension);
                attachmentParentDto.PostId = postEntity.Id;
                attachmentParentDto.FileExtension = extension;
                attachmentParentDto.FileName = fileName;
                attachmentParentDto.Size = attachment.Length;
                attachmentParentDto.Hash = _cryptoService.HashHex(attachment.OpenReadStream());
                string blobName;

                switch (attachmentParentDto)
                {
                    case AudioDto audioDto:
                    {
                        var audioEntity = MapDtoToNewEntity<AudioDto, Audio>(audioDto);
                        audioEntity.Post = postEntity;
                        await _context.Audio.AddAsync(audioEntity);
                        await _context.SaveChangesAsync();
                        blobName = audioEntity.Id.ToString();
                        break;
                    }
                    case PictureDto pictureDto:
                    {
                        if (_attachmentCategorizer.IsPictureExtensionSupported(pictureDto.FileExtension))
                        {
                            using (var image = Image.Load(attachment.OpenReadStream()))
                            {
                                pictureDto.Width = image.Width;
                                pictureDto.Height = image.Height;
                                        
                                var pictureEntity = MapDtoToNewEntity<PictureDto, Picture>(pictureDto);
                                pictureEntity.Post = postEntity;
                                await _context.Pictures.AddAsync(pictureEntity);
                                await _context.SaveChangesAsync();
                                blobName = pictureEntity.Id.ToString();
                            
                                // generate thumbnail
                                var thumbnail = _thumbnailGenerator.GenerateThumbnail(
                                    image,
                                    _hikkabaConfiguration.ThumbnailsMaxWidth,
                                    _hikkabaConfiguration.ThumbnailsMaxHeight);
                                await _storageProvider.SaveBlobStreamAsync(containerName + Defaults.ThumbnailPostfix,
                                    blobName,
                                    thumbnail.Image);
                            }
                        }
                        else
                        {
                            var pictureEntity = MapDtoToNewEntity<PictureDto, Picture>(pictureDto);
                            pictureEntity.Post = postEntity;
                            await _context.Pictures.AddAsync(pictureEntity);
                            await _context.SaveChangesAsync();
                            blobName = pictureEntity.Id.ToString();
                            
                            // save original image as thumbnail
                            await _storageProvider.SaveBlobStreamAsync(containerName + Defaults.ThumbnailPostfix,
                                blobName,
                                attachment.OpenReadStream());
                        }
                        break;
                    }
                    case VideoDto videoDto:
                    {
                        var videoEntity = MapDtoToNewEntity<VideoDto, Video>(videoDto);
                        videoEntity.Post = postEntity;
                        await _context.Video.AddAsync(videoEntity);
                        await _context.SaveChangesAsync();
                        blobName = videoEntity.Id.ToString();
                        break;
                    }
                    case DocumentDto documentDto:
                    {
                        var documentEntity = MapDtoToNewEntity<DocumentDto, Document>(documentDto);
                        documentEntity.Post = postEntity;
                        await _context.Documents.AddAsync(documentEntity);
                        await _context.SaveChangesAsync();
                        blobName = documentEntity.Id.ToString();
                        break;
                    }
                    default:
                        throw new Exception($"Unknown attachment type: {attachmentParentDto.GetType().Name}");
                }
                
                if (string.IsNullOrWhiteSpace(blobName))
                {
                    throw new Exception($"{nameof(blobName)} is null or whitespace");
                }
                await _storageProvider.SaveBlobStreamAsync(containerName, blobName, attachment.OpenReadStream());
            }
        }
        
        public async Task<ThreadPostCreateResultDto> CreateThreadPostAsync(IFormFileCollection attachments, ThreadPostCreateDto dto, bool createNewThread)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            if (createNewThread)
            {
                var thread = MapDtoToNewEntity<ThreadDto, Thread>(dto.Thread);
                thread.Category = _context.GetLocalOrAttach<Category>(dto.Category.Id);
                await _context.Threads.AddAsync(thread);
                await _context.SaveChangesAsync();
                dto.Post.ThreadId = thread.Id;
            }
            var postingPermissionDto = await _banService.IsPostingAllowedAsync(dto.Post.ThreadId, dto.Post.UserIpAddress);
            if (!postingPermissionDto.IsPostingAllowed)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, postingPermissionDto.Ban?.Reason);
            }
            else
            {
                var postEntity = MapDtoToNewEntity<PostDto, Post>(dto.Post);
                postEntity.Thread = _context.GetLocalOrAttach<Thread>(dto.Post.ThreadId);
                await _context.Posts.AddAsync(postEntity);
                await _context.SaveChangesAsync();
                
                var containerName = dto.Post.ThreadId.ToString();
                if (string.IsNullOrWhiteSpace(containerName))
                {
                    throw new Exception($"{nameof(containerName)} is null or whitespace");
                }

                if (attachments != null && attachments.Any())
                {
                    await CreateAttachmentsAsync(containerName, postEntity, attachments);
                }

                await transaction.CommitAsync();
                return new ThreadPostCreateResultDto
                {
                    PostId = postEntity.Id, 
                    ThreadId = dto.Post.ThreadId,
                };
            }
        }
        
        public async Task EditAsync(ThreadDto dto)
        {
            var existingEntity = await _context.Threads.FirstOrDefaultAsync(e => e.Id == dto.Id);
            MapDtoToExistingEntity(dto, existingEntity);
            existingEntity.Category = _context.GetLocalOrAttach<Category>(dto.CategoryId);
            await _context.SaveChangesAsync();
        }
        
        public async Task<BasePagedList<ThreadDto>> PagedListAsync(Expression<Func<Thread, bool>> where, PageDto page = null)
        {
            page ??= new PageDto();

            var query = _context.Threads
                .Where(where)
                .OrderByDescending(thread => thread.IsPinned)
                .ThenByDescending(thread => thread.Posts
                    .OrderBy(post => post.Created)
                    .Take(thread.BumpLimit)
                    .LastOrDefault(post => !post.IsSageEnabled && !post.IsDeleted)
                    .Created);

            var pagedQuery = query
                .Skip(page.Skip)
                .Take(page.PageSize);

            var entityList = await pagedQuery.ToListAsync();
            var dtoList = MapEntityListToDtoList<ThreadDto, Thread>(entityList);
            var pagedList = new BasePagedList<ThreadDto>
            {
                CurrentPage = page,
                TotalItemsCount = query.Count(),
                CurrentPageItems = dtoList
            };
            return pagedList;
        }

        public async Task SetIsPinnedAsync(TPrimaryKey id, bool newValue)
        {
            var entity = _context.GetLocalOrAttach<Thread>(id);
            entity.IsPinned = newValue;
            _context.Entry(entity).Property(e => e.IsPinned).IsModified = true;
            await _context.SaveChangesAsync();
        }

        public async Task SetIsClosedAsync(TPrimaryKey id, bool newValue)
        {
            var entity = _context.GetLocalOrAttach<Thread>(id);
            entity.IsClosed = newValue;
            _context.Entry(entity).Property(e => e.IsClosed).IsModified = true;
            await _context.SaveChangesAsync();
        }

        public async Task SetIsDeletedAsync(TPrimaryKey id, bool newValue)
        {
            var entity = _context.GetLocalOrAttach<Thread>(id);
            entity.IsDeleted = newValue;
            _context.Entry(entity).Property(e => e.IsDeleted).IsModified = true;
            await _context.SaveChangesAsync();
        }
    }
}
