using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Configuration;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Data;
using Hikkaba.Common.Dto;
using Hikkaba.Common.Dto.Attachments;
using Hikkaba.Common.Dto.Attachments.Base;
using Hikkaba.Common.Entities;
using Hikkaba.Common.Exceptions;
using Hikkaba.Common.Storage;
using Hikkaba.Service.Attachments;
using Hikkaba.Service.Base;
using ImageSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwentyTwenty.Storage;

namespace Hikkaba.Service
{
    public interface IPostService : IBaseModeratedMutableEntityService<PostDto, Post, Guid>
    {
        Task<Guid> CreateAsync(IFormFileCollection attachments, PostDto dto);
    }

    public class PostService : BaseModeratedMutableEntityService<PostDto, Post, Guid>, IPostService
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
            ILocalStorageProviderFactory localStorageProviderFactory,
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
            _storageProvider = localStorageProviderFactory.CreateLocalStorageProvider();
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

        protected override IQueryable<Post> GetDbSetWithReferences(ApplicationDbContext context)
        {
            return context.Posts
                    .Include(post => post.Thread)
                        .ThenInclude(thread => thread.Category)
                    .Include(post => post.Audio)
                    .Include(post => post.Documents)
                    .Include(post => post.Notices)
                        .ThenInclude(entity => entity.Author)
                    .Include(post => post.Pictures)
                    .Include(post => post.Video);
        }

        protected override void LoadReferenceFields(ApplicationDbContext context, Post entityEntry)
        {
            context.Entry(entityEntry).Reference(post => post.Thread).Load();
            context.Entry(entityEntry).Collection(post => post.Audio).Load();
            context.Entry(entityEntry).Collection(post => post.Documents).Load();
            context.Entry(entityEntry).Collection(post => post.Notices).Load();
            context.Entry(entityEntry).Collection(post => post.Pictures).Load();
            context.Entry(entityEntry).Collection(post => post.Video).Load();
        }

        protected override Guid GetCategoryId(Post entity)
        {
            return entity.Thread.Category.Id;
        }

        protected override IBaseManyToManyService<Guid, Guid> GetManyToManyService()
        {
            return _categoryToModeratorService;
        }

        public async Task<Guid> CreateAsync(IFormFileCollection attachments, PostDto dto)
        {
            var isPostingAllowed = await _banService.IsPostingAllowedAsync(dto.ThreadId, dto.UserIpAddress);
            if (!isPostingAllowed.Item1)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden, isPostingAllowed.Item2);
            }
            else if (attachments == null)
            {
                return await CreateAsync(dto);
            }
            else
            {
                var postId = await CreateAsync(dto);

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

                        var attachmentParentDto = _attachmentCategorizer.GetAttachmentTypeByFileName(attachment.FileName);
                        attachmentParentDto.PostId = postId;
                        attachmentParentDto.FileExtension = Path.GetExtension(attachment.FileName).TrimStart('.');
                        attachmentParentDto.FileName = Path.GetFileNameWithoutExtension(attachment.FileName);
                        attachmentParentDto.Size = attachment.Length;
                        attachmentParentDto.Hash = _cryptoService.HashHex(attachment.OpenReadStream());

                        if (attachmentParentDto is AudioDto)
                        {
                            var attachmentDto = (AudioDto)attachmentParentDto;
                            blobName = (await _audioService.CreateAsync(attachmentDto)).ToString();
                        }
                        else if (attachmentParentDto is PictureDto)
                        {
                            var attachmentDto = (PictureDto)attachmentParentDto;

                            if (_attachmentCategorizer.IsPictureExtensionSupported(attachmentParentDto.FileExtension))
                            {
                                // generate thumbnail if such file extension is supported
                                Image image = new Image(attachment.OpenReadStream());
                                attachmentDto.Width = image.Width;
                                attachmentDto.Height = image.Height;
                                blobName = (await _pictureService.CreateAsync(attachmentDto)).ToString();

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
                                blobName = (await _pictureService.CreateAsync(attachmentDto)).ToString();
                                await _storageProvider.SaveBlobStreamAsync(
                                        containerName + Defaults.ThumbnailPostfix,
                                        blobName,
                                        attachment.OpenReadStream());
                            }
                        }
                        else if (attachmentParentDto is VideoDto)
                        {
                            var attachmentDto = (VideoDto)attachmentParentDto;
                            blobName = (await _videoService.CreateAsync(attachmentDto)).ToString();
                        }
                        else if (attachmentParentDto is DocumentDto)
                        {
                            var attachmentDto = (DocumentDto)attachmentParentDto;
                            blobName = (await _documentService.CreateAsync(attachmentDto)).ToString();
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