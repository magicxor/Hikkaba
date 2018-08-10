using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Exceptions;
using Hikkaba.Models.Configuration;
using Hikkaba.Models.Dto;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Services.Attachments;
using Hikkaba.Services.Base.Concrete;
using Hikkaba.Services.Base.Generic;
using Hikkaba.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using TwentyTwenty.Storage;
using TPrimaryKey = System.Guid;

namespace Hikkaba.Services
{
    public interface IPostService : IBaseModeratedMutableEntityService<PostDto, Post>
    {
        Task<TPrimaryKey> CreateAsync(IFormFileCollection attachments, PostDto dto);
        Task EditAsync(PostDto dto, TPrimaryKey currentUserId);
    }

    public class PostService : BaseModeratedMutableEntityService<PostDto, Post>, IPostService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly ILogger<PostService> _logger;
        private readonly IBanService _banService;
        private readonly HikkabaConfiguration _hikkabaConfiguration;
        private readonly ICryptoService _cryptoService;
        private readonly IAudioService _audioService;
        private readonly IDocumentService _documentService;
        private readonly IPictureService _pictureService;
        private readonly IVideoService _videoService;
        private readonly IThumbnailGenerator _thumbnailGenerator;
        private readonly IAttachmentCategorizer _attachmentCategorizer;
        private readonly ICategoryToModeratorService _categoryToModeratorService;

        public PostService(
            IStorageProviderFactory storageProviderFactory,
            ILogger<PostService> logger,
            IOptions<HikkabaConfiguration> settings,
            IBanService banService,
            ICryptoService cryptoService,
            IAudioService audioService,
            IDocumentService documentService,
            IPictureService pictureService,
            IVideoService videoService,
            IThumbnailGenerator thumbnailGenerator,
            IAttachmentCategorizer attachmentCategorizer,
            IMapper mapper,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ICategoryToModeratorService categoryToModeratorService) : base(mapper, context, userManager)
        {
            _storageProvider = storageProviderFactory.CreateStorageProvider();
            _logger = logger;
            _banService = banService;
            _hikkabaConfiguration = settings.Value;
            _cryptoService = cryptoService;
            _audioService = audioService;
            _documentService = documentService;
            _pictureService = pictureService;
            _videoService = videoService;
            _thumbnailGenerator = thumbnailGenerator;
            _attachmentCategorizer = attachmentCategorizer;
            _categoryToModeratorService = categoryToModeratorService;
        }

        protected override DbSet<Post> GetDbSet(ApplicationDbContext context)
        {
            return context.Posts;
        }

        protected override TPrimaryKey GetCategoryId(Post entity)
        {
            return entity.Thread.Category.Id;
        }

        protected override IBaseManyToManyService<TPrimaryKey, TPrimaryKey> GetManyToManyService()
        {
            return _categoryToModeratorService;
        }

        public async Task EditAsync(PostDto dto, TPrimaryKey currentUserId)
        {
            await base.EditAsync(dto, currentUserId, post =>
            {
                var audioList = Context.Audio.Where(entity => entity.Post.Id == dto.Id);
                var pictureList = Context.Pictures.Where(entity => entity.Post.Id == dto.Id);
                var videoList = Context.Video.Where(entity => entity.Post.Id == dto.Id);
                var documentsList = Context.Documents.Where(entity => entity.Post.Id == dto.Id);
                var noticesList = Context.Notices.Where(entity => entity.Post.Id == dto.Id);
            });
        }

        public async Task<TPrimaryKey> CreateAsync(IFormFileCollection attachments, PostDto dto)
        {
            var isPostingAllowed = await _banService.IsPostingAllowedAsync(dto.ThreadId, dto.UserIpAddress);
            if (!isPostingAllowed.Item1)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, isPostingAllowed.Item2);
            }
            else if (attachments == null)
            {
                return await CreateAsync(dto, (post) =>
                {
                    post.Thread = Context.Threads.FirstOrDefault(thread => thread.Id == dto.ThreadId);
                });
            }
            else
            {
                var postId = await CreateAsync(dto, (post) =>
                {
                    post.Thread = Context.Threads.FirstOrDefault(thread => thread.Id == dto.ThreadId);
                });

                try
                {
                    var containerName = dto.ThreadId.ToString();
                    if (string.IsNullOrWhiteSpace(containerName))
                    {
                        throw new Exception($"{nameof(containerName)} is null or whitespace");
                    }

                    foreach (var attachment in attachments)
                    {
                        string blobName;

                        var attachmentParentDto = _attachmentCategorizer.CreateAttachmentDto(attachment.FileName);
                        attachmentParentDto.PostId = postId;
                        attachmentParentDto.FileExtension = Path.GetExtension(attachment.FileName)?.TrimStart('.');
                        attachmentParentDto.FileName = Path.GetFileNameWithoutExtension(attachment.FileName);
                        attachmentParentDto.Size = attachment.Length;
                        attachmentParentDto.Hash = _cryptoService.HashHex(attachment.OpenReadStream());

                        if (attachmentParentDto is AudioDto audioDto)
                        {
                            blobName = (await _audioService.CreateAsync(audioDto, entity =>
                            {
                                entity.Post = Context.Posts.FirstOrDefault(post => post.Id == postId);
                            })).ToString();
                        }
                        else if (attachmentParentDto is PictureDto pictureDto)
                        {
                            if (_attachmentCategorizer.IsPictureExtensionSupported(pictureDto.FileExtension))
                            {
                                // generate thumbnail if such file extension is supported
                                var image = Image.Load(attachment.OpenReadStream());
                                pictureDto.Width = image.Width;
                                pictureDto.Height = image.Height;
                                blobName = (await _pictureService.CreateAsync(pictureDto, entity =>
                                {
                                    entity.Post = Context.Posts.FirstOrDefault(post => post.Id == postId);
                                })).ToString();

                                var thumbnail = _thumbnailGenerator.GenerateThumbnail(
                                    image,
                                    _hikkabaConfiguration.ThumbnailsMaxWidth,
                                    _hikkabaConfiguration.ThumbnailsMaxHeight);
                                await _storageProvider.SaveBlobStreamAsync(
                                        containerName + Defaults.ThumbnailPostfix,
                                        blobName,
                                        thumbnail.Image);
                            }
                            else
                            {
                                // otherwise save the same image as thumbnail
                                blobName = (await _pictureService.CreateAsync(pictureDto, entity =>
                                {
                                    entity.Post = Context.Posts.FirstOrDefault(post => post.Id == postId);
                                })).ToString();
                                await _storageProvider.SaveBlobStreamAsync(
                                        containerName + Defaults.ThumbnailPostfix,
                                        blobName,
                                        attachment.OpenReadStream());
                            }
                        }
                        else if (attachmentParentDto is VideoDto videoDto)
                        {
                            blobName = (await _videoService.CreateAsync(videoDto, entity =>
                            {
                                entity.Post = Context.Posts.FirstOrDefault(post => post.Id == postId);
                            })).ToString();
                        }
                        else if (attachmentParentDto is DocumentDto documentDto)
                        {
                            blobName = (await _documentService.CreateAsync(documentDto, entity =>
                            {
                                entity.Post = Context.Posts.FirstOrDefault(post => post.Id == postId);
                            })).ToString();
                        }
                        else
                        {
                            throw new Exception($"Unknown attachment type: {attachmentParentDto.GetType().Name}");
                        }
                        if (string.IsNullOrWhiteSpace(blobName))
                        {
                            throw new Exception($"{nameof(blobName)} is null or whitespace");
                        }
                        await _storageProvider.SaveBlobStreamAsync(containerName, blobName, attachment.OpenReadStream());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex + $" Post {postId} will be deleted.");
                    await DeleteAsync(postId);
                    throw;
                }

                return postId;
            }
        }
    }
}