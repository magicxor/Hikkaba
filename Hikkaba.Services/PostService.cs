using TPrimaryKey = System.Guid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Data.Extensions;
using Hikkaba.Infrastructure.Exceptions;
using Hikkaba.Models.Configuration;
using Hikkaba.Models.Dto;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Services.Base.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using TwentyTwenty.Storage;

namespace Hikkaba.Services
{
    public interface IPostService
    {
        Task<PostDto> GetAsync(TPrimaryKey id);
        
        Task<IList<PostDto>> ListAsync<TOrderKey>(
            Expression<Func<Post, bool>> where = null, 
            Expression<Func<Post, TOrderKey>> orderBy = null, 
            bool isDescending = false);

        Task<BasePagedList<PostDto>> PagedListAsync<TOrderKey>(
            Expression<Func<Post, bool>> where = null,
            Expression<Func<Post, TOrderKey>> orderBy = null, bool isDescending = false,
            PageDto page = null);
        
        Task<TPrimaryKey> CreateAsync(IFormFileCollection attachments, PostDto postDto);
        
        Task EditAsync(PostDto dto);
        
        Task DeleteAsync(TPrimaryKey id);
    }

    public class PostService : BaseEntityService, IPostService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IBanService _banService;
        private readonly HikkabaConfiguration _hikkabaConfiguration;
        private readonly ICryptoService _cryptoService;
        private readonly IThumbnailGenerator _thumbnailGenerator;
        private readonly IAttachmentCategorizer _attachmentCategorizer;
        private readonly ApplicationDbContext _context;

        public PostService(
            IStorageProvider storageProvider,
            IOptions<HikkabaConfiguration> settings,
            IBanService banService,
            ICryptoService cryptoService,
            IThumbnailGenerator thumbnailGenerator,
            IAttachmentCategorizer attachmentCategorizer,
            IMapper mapper,
            ApplicationDbContext context) : base(mapper)
        {
            _storageProvider = storageProvider;
            _banService = banService;
            _hikkabaConfiguration = settings.Value;
            _cryptoService = cryptoService;
            _thumbnailGenerator = thumbnailGenerator;
            _attachmentCategorizer = attachmentCategorizer;
            _context = context;
        }
        
        private IQueryable<Post> Query<TOrderKey>(Expression<Func<Post, bool>> where = null, Expression<Func<Post, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = _context.Posts.Include(p => p.Attachments).AsQueryable();

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
        
        public async Task<PostDto> GetAsync(TPrimaryKey id)
        {
            var entity = await _context.Posts.FirstOrDefaultAsync(u => u.Id == id);
            var dto = MapEntityToDto<PostDto, Post>(entity);
            return dto;
        }

        public async Task<IList<PostDto>> ListAsync<TOrderKey>(Expression<Func<Post, bool>> where = null, Expression<Func<Post, TOrderKey>> orderBy = null, bool isDescending = false)
        {
            var query = Query(where, orderBy, isDescending);
            var entityList = await query.ToListAsync();
            var dtoList = MapEntityListToDtoList<PostDto, Post>(entityList);
            return dtoList;
        }

        public async Task<BasePagedList<PostDto>> PagedListAsync<TOrderKey>(Expression<Func<Post, bool>> where = null, Expression<Func<Post, TOrderKey>> orderBy = null, bool isDescending = false, PageDto page = null)
        {
            page = page ?? new PageDto();

            var query = Query(where, orderBy, isDescending);

            var pageQuery = query.Skip(page.Skip).Take(page.PageSize);

            var entityList = await pageQuery.ToListAsync();
            var dtoList = MapEntityListToDtoList<PostDto, Post>(entityList);
            var pagedList = new BasePagedList<PostDto>
            {
                TotalItemsCount = query.Count(),
                CurrentPage = page,
                CurrentPageItems = dtoList,
            };
            return pagedList;
        }

        public async Task EditAsync(PostDto dto)
        {
            var existingEntity = await _context.Posts.FirstOrDefaultAsync(p => p.Id == dto.Id);
            MapDtoToExistingEntity(dto, existingEntity);
            await _context.SaveChangesAsync();
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
        
        public async Task<TPrimaryKey> CreateAsync(IFormFileCollection attachments, PostDto postDto)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                var postingPermissionDto = await _banService.IsPostingAllowedAsync(postDto.ThreadId, postDto.UserIpAddress);
                if (!postingPermissionDto.IsPostingAllowed)
                {
                    throw new HttpResponseException(HttpStatusCode.Forbidden, postingPermissionDto.Ban?.Reason);
                }
                else
                {
                    var postEntity = MapDtoToNewEntity<PostDto, Post>(postDto);
                    postEntity.Thread = _context.GetLocalOrAttach<Thread>(postDto.ThreadId);
                    await _context.Posts.AddAsync(postEntity);
                    await _context.SaveChangesAsync();
                    
                    try
                    {
                        var containerName = postDto.ThreadId.ToString();
                        if (string.IsNullOrWhiteSpace(containerName))
                        {
                            throw new Exception($"{nameof(containerName)} is null or whitespace");
                        }

                        if (attachments != null && attachments.Any())
                        {
                            await CreateAttachmentsAsync(containerName, postEntity, attachments);
                        }

                        await transaction.CommitAsync();
                        return postEntity.Id;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
        
        public async Task DeleteAsync(TPrimaryKey id)
        {
            var entity = _context.GetLocalOrAttach<Post>(id);
            entity.IsDeleted = true;
            _context.Entry(entity).Property(e => e.IsDeleted).IsModified = true;
            await _context.SaveChangesAsync();
        }
    }
}