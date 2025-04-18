﻿using System.Net;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Attachments.Concrete;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Error;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.QueryableExtensions;
using Hikkaba.Infrastructure.Repositories.Telemetry;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf.Types;
using Thinktecture;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class ThreadRepository : IThreadRepository
{
    private readonly ILogger<ThreadRepository> _logger;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly TimeProvider _timeProvider;
    private readonly IAttachmentRepository _attachmentRepository;

    public ThreadRepository(
        ILogger<ThreadRepository> logger,
        ApplicationDbContext applicationDbContext,
        TimeProvider timeProvider,
        IAttachmentRepository attachmentRepository)
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
        _timeProvider = timeProvider;
        _attachmentRepository = attachmentRepository;
    }

    public async Task<CategoryThreadModel?> GetCategoryThreadAsync(CategoryThreadFilter filter, CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.ThreadSource.StartActivity();

        return await _applicationDbContext.Threads
            .TagWithCallSite()
            .Include(t => t.Category)
            .Where(t => (filter.IncludeDeleted || (!t.IsDeleted && !t.Category.IsDeleted))
                && t.Category.Alias == filter.CategoryAlias
                && t.Id == filter.ThreadId)
            .OrderByDescending(t => t.Id)
            .Select(t => new CategoryThreadModel
            {
                ThreadId = t.Id,
                CategoryAlias = t.Category.Alias,
                CategoryName = t.Category.Name,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ThreadDetailsRequestModel?> GetThreadDetailsAsync(long threadId, bool includeDeleted, CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.ThreadSource.StartActivity();

        var posts = await _applicationDbContext.Posts
            .TagWithCallSite()
            .Include(post => post.Thread)
            .ThenInclude(thread => thread.Category)
            .Include(post => post.Audios)
            .Include(post => post.Documents)
            .Include(post => post.Notices)
            .Include(post => post.Pictures)
            .Include(post => post.Videos)
            .Include(post => post.RepliesToThisMentionedPost)
            .Where(p => p.ThreadId == threadId
                        && (includeDeleted || (!p.IsDeleted && !p.Thread.IsDeleted && !p.Thread.Category.IsDeleted)))
            .OrderBy(post => post.CreatedAt)
            .ThenBy(post => post.Id)
            .GetDetailsModel()
            .ToListAsync(cancellationToken);

        var thread = await _applicationDbContext.Threads
            .TagWithCallSite()
            .Include(thread => thread.Category)
            .Where(thread => thread.Id == threadId
                             && (includeDeleted || (!thread.IsDeleted && !thread.Category.IsDeleted)))
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (thread is null || posts.Count == 0)
            return null;

        return new ThreadDetailsRequestModel
        {
            Id = thread.Id,
            IsDeleted = thread.IsDeleted,
            CreatedAt = thread.CreatedAt,
            ModifiedAt = thread.ModifiedAt,
            Title = thread.Title,
            IsPinned = thread.IsPinned,
            IsClosed = thread.IsClosed,
            IsCyclic = thread.IsCyclic,
            BumpLimit = thread.BumpLimit,
            ShowThreadLocalUserHash = thread.Category.ShowThreadLocalUserHash,
            CategoryId = thread.Category.Id,
            CategoryAlias = thread.Category.Alias,
            CategoryName = thread.Category.Name,
            PostCount = posts.Count,
            Posts = posts,
        };
    }

    public async Task<PagedResult<ThreadPreviewModel>> ListThreadPreviewsPaginatedAsync(
        ThreadPreviewFilter filter,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.ThreadSource.StartActivity();

        var threadQuery = _applicationDbContext.Threads
            .TagWithCallSite()
            .Include(thread => thread.Category)
            .AsQueryable()
            .AsSingleQuery();

        if (!string.IsNullOrEmpty(filter.CategoryAlias))
        {
            threadQuery = threadQuery.Where(thread => thread.Category.Alias == filter.CategoryAlias);
        }

        threadQuery = filter.IncludeDeleted
            ? threadQuery.IgnoreQueryFilters()
            : threadQuery.Where(thread => !thread.IsDeleted && !thread.Category.IsDeleted);

        var resultQuery = threadQuery
            .Select(thread => new
            {
                Thread = thread,
                Category = thread.Category,
                BumpLimit = thread.BumpLimit > 0 ? thread.BumpLimit : thread.Category.DefaultBumpLimit,
                PostCount = thread.Posts.Count(post => filter.IncludeDeleted || !post.IsDeleted),
            })
            .Select(x => new
            {
                Thread = x.Thread,
                Category = x.Category,
                LastPostCreatedAt = x.Thread.Posts
                    .OrderBy(p => p.CreatedAt)
                    .ThenBy(p => p.Id)
                    .Take(x.BumpLimit)
                    .Where(p => !p.IsSageEnabled && !p.IsDeleted)
                    .Max(p => p.CreatedAt),
                Posts = x.Thread.Posts
                    .Where(p => filter.IncludeDeleted || !p.IsDeleted)
                    .OrderBy(p => p.CreatedAt)
                    /* take the OP-post (the first post) */
                    .Take(1)
                    .Union(x.Thread.Posts
                        .Where(p => filter.IncludeDeleted || !p.IsDeleted)
                        .OrderByDescending(p => p.CreatedAt)
                        /* take the 3 last posts */
                        .Take(Defaults.LatestPostsCountInThreadPreview))
                    .ToList(),
                PostCount = x.PostCount,
            })
            .Where(thread => filter.IncludeDeleted || thread.PostCount > 0)
            .Select(g => new ThreadPreviewModel
            {
                Id = g.Thread.Id,
                IsDeleted = g.Thread.IsDeleted,
                CreatedAt = g.Thread.CreatedAt,
                ModifiedAt = g.Thread.ModifiedAt,
                LastPostCreatedAt = g.LastPostCreatedAt,
                Title = g.Thread.Title,
                IsPinned = g.Thread.IsPinned,
                IsClosed = g.Thread.IsClosed,
                IsCyclic = g.Thread.IsCyclic,
                BumpLimit = g.Thread.BumpLimit,
                ShowThreadLocalUserHash = g.Category.ShowThreadLocalUserHash,
                CategoryId = g.Category.Id,
                CategoryAlias = g.Category.Alias,
                CategoryName = g.Category.Name,
                PostCount = g.PostCount,
                Posts = g.Posts.Select(post => new PostDetailsModel
                    {
                        Index = EF.Functions.RowNumber(EF.Functions.OrderByDescending(post.CreatedAt)
                            .ThenByDescending(post.Id)),
                        Id = post.Id,
                        IsDeleted = post.IsDeleted,
                        CreatedAt = post.CreatedAt,
                        ModifiedAt = post.ModifiedAt,
                        IsSageEnabled = post.IsSageEnabled,
                        MessageHtml = post.MessageHtml,
                        UserIpAddress = post.UserIpAddress,
                        UserAgent = post.UserAgent,
                        Audio = post.Audios
                            .Select(attachment => new AudioModel
                            {
                                Id = attachment.Id,
                                PostId = attachment.PostId,
                                ThreadId = post.ThreadId,
                                BlobId = attachment.BlobId,
                                BlobContainerId = post.BlobContainerId,
                                FileName = attachment.FileNameWithoutExtension,
                                FileExtension = attachment.FileExtension,
                                FileSize = attachment.FileSize,
                                FileContentType = attachment.FileContentType,
                                FileHash = attachment.FileHash,
                                Title = attachment.Title,
                                Album = attachment.Album,
                                Artist = attachment.Artist,
                                DurationSeconds = attachment.DurationSeconds,
                            })
                            .ToList(),
                        Documents = post.Documents
                            .Select(attachment => new DocumentModel
                            {
                                Id = attachment.Id,
                                PostId = attachment.PostId,
                                ThreadId = post.ThreadId,
                                BlobId = attachment.BlobId,
                                BlobContainerId = post.BlobContainerId,
                                FileName = attachment.FileNameWithoutExtension,
                                FileExtension = attachment.FileExtension,
                                FileSize = attachment.FileSize,
                                FileContentType = attachment.FileContentType,
                                FileHash = attachment.FileHash,
                            })
                            .ToList(),
                        Notices = post.Notices
                            .Select(attachment => new NoticeModel
                            {
                                Id = attachment.Id,
                                PostId = attachment.PostId,
                                ThreadId = post.ThreadId,
                                Text = attachment.Text,
                            })
                            .ToList(),
                        Pictures = post.Pictures
                            .Select(attachment => new PictureModel
                            {
                                Id = attachment.Id,
                                PostId = attachment.PostId,
                                ThreadId = post.ThreadId,
                                BlobId = attachment.BlobId,
                                BlobContainerId = post.BlobContainerId,
                                FileName = attachment.FileNameWithoutExtension,
                                FileExtension = attachment.FileExtension,
                                FileSize = attachment.FileSize,
                                FileContentType = attachment.FileContentType,
                                FileHash = attachment.FileHash,
                                Width = attachment.Width,
                                Height = attachment.Height,
                                ThumbnailExtension = attachment.ThumbnailExtension,
                                ThumbnailWidth = attachment.ThumbnailWidth,
                                ThumbnailHeight = attachment.ThumbnailHeight,
                            })
                            .ToList(),
                        Video = post.Videos
                            .Select(attachment => new VideoModel
                            {
                                Id = attachment.Id,
                                PostId = attachment.PostId,
                                ThreadId = post.ThreadId,
                                BlobId = attachment.BlobId,
                                BlobContainerId = post.BlobContainerId,
                                FileName = attachment.FileNameWithoutExtension,
                                FileExtension = attachment.FileExtension,
                                FileSize = attachment.FileSize,
                                FileContentType = attachment.FileContentType,
                                FileHash = attachment.FileHash,
                            })
                            .ToList(),
                        ThreadId = post.ThreadId,
                        ShowThreadLocalUserHash = g.Category.ShowThreadLocalUserHash,
                        ShowCountry = g.Category.ShowCountry,
                        ShowOs = g.Category.ShowOs,
                        ShowBrowser = g.Category.ShowBrowser,
                        ThreadLocalUserHash = post.ThreadLocalUserHash,
                        CategoryAlias = g.Category.Alias,
                        CategoryId = g.Category.Id,
                        Replies = post.RepliesToThisMentionedPost
                            .Where(r => !r.Post.IsDeleted && !r.Reply.IsDeleted)
                            .Select(r => r.ReplyId)
                            .ToList(),
                    })
                    .OrderBy(x => x.CreatedAt)
                    .ThenBy(x => x.Id)
                    .ToList(),
            });

        var totalCount = await resultQuery.CountAsync(cancellationToken);

        var data = await resultQuery
            .ApplyOrderByAndPaging(filter, x => x.LastPostCreatedAt, OrderByDirection.Desc)
            .ToListAsync(cancellationToken);

        // calculate real post index
        foreach (var thread in data)
        {
            foreach (var post in thread.Posts)
            {
                post.Index = post.Id == thread.Posts[0].Id
                    ? 1
                    : thread.PostCount - (post.Index - 1);
            }
        }

        return new PagedResult<ThreadPreviewModel>(data, filter, totalCount);
    }

    public async Task<ThreadPostCreateResultModel> CreateThreadAsync(
        ThreadCreateExtendedRequestModel createRequestModel,
        FileAttachmentContainerCollection inputFiles,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.ThreadSource.StartActivity();
        _logger.LogInformation(
            LogEventIds.CreatingThread,
            "Creating thread in category {CategoryAlias}. Attachment count: {AttachmentCount}, BlobContainerId: {BlobContainerId}",
            createRequestModel.BaseModel.CategoryAlias,
            inputFiles.Count,
            createRequestModel.BaseModel.BlobContainerId);

        var attachments = _attachmentRepository.ToAttachmentEntities(inputFiles);

        var category = await _applicationDbContext.Categories
            .TagWithCallSite()
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Alias,
                x.DefaultBumpLimit,
                x.MaxThreadCount,
                x.IsDeleted,
                ThreadCount = x.Threads.Count,
            })
            .FirstOrDefaultAsync(x => x.Alias == createRequestModel.BaseModel.CategoryAlias && !x.IsDeleted, cancellationToken);

        if (category is null)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ErrorMessage = $"Category with alias {createRequestModel.BaseModel.CategoryAlias} not found",
            };
        }

        var deletedBlobContainerIds = new List<Guid>();

        if (category.ThreadCount >= category.MaxThreadCount)
        {
            var threadsToBeDeleted = await _applicationDbContext.Threads
                .TagWithCallSite()
                .Include(t => t.Posts)
                .Where(t => t.CategoryId == category.Id)
                .OrderBy(t => t.CreatedAt)
                .ThenBy(t => t.Id)
                .Take(1) /* delete the oldest thread */
                .ToListAsync(cancellationToken);

            deletedBlobContainerIds = threadsToBeDeleted.SelectMany(t => t.Posts.Select(p => p.BlobContainerId)).ToList();

            _logger.LogInformation(
                LogEventIds.DeletingOldThreads,
                "Deleting old thread(s) in category {CategoryAlias}. Id: {CategoryId}, ThreadCount: {ThreadCount}, MaxThreadCount: {MaxThreadCount}, Blob containers to be deleted: {BlobContainerCount}",
                category.Alias,
                category.Id,
                category.ThreadCount,
                category.MaxThreadCount,
                deletedBlobContainerIds.Count);

            _applicationDbContext.Threads.RemoveRange(threadsToBeDeleted);
        }

        var post = new Post
        {
            BlobContainerId = createRequestModel.BaseModel.BlobContainerId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = false,
            MessageText = createRequestModel.BaseModel.MessageText,
            MessageHtml = createRequestModel.BaseModel.MessageHtml,
            UserIpAddress = createRequestModel.BaseModel.UserIpAddress,
            UserAgent = createRequestModel.BaseModel.UserAgent,
            ThreadLocalUserHash = createRequestModel.ThreadLocalUserHash,
            Audios = attachments.Audios,
            Documents = attachments.Documents,
            Pictures = attachments.Pictures,
            Videos = attachments.Videos,
        };

        var thread = new Thread
        {
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            Title = createRequestModel.BaseModel.ThreadTitle,
            BumpLimit = category.DefaultBumpLimit > 0 ? category.DefaultBumpLimit : Defaults.DefaultBumpLimit,
            Salt = createRequestModel.ThreadSalt,
            CategoryId = category.Id,
            Posts = [post],
        };

        _applicationDbContext.Threads.Add(thread);

        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        return new ThreadPostCreateSuccessResultModel
        {
            ThreadId = thread.Id,
            PostId = post.Id,
            DeletedBlobContainerIds = deletedBlobContainerIds,
        };
    }

    public async Task<ThreadPatchResultModel> EditThreadAsync(ThreadEditRequestModel editRequestModel, CancellationToken cancellationToken)
    {
        var thread = await _applicationDbContext.Threads
            .TagWithCallSite()
            .OrderBy(t => t.Id)
            .FirstOrDefaultAsync(t => t.Id == editRequestModel.Id, cancellationToken);

        if (thread is null)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ErrorMessage = $"Thread with id {editRequestModel.Id} not found",
            };
        }

        thread.Title = editRequestModel.Title;
        thread.BumpLimit = editRequestModel.BumpLimit;
        thread.ModifiedAt = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        return default(Success);
    }

    public async Task<ThreadPatchResultModel> SetThreadPinnedAsync(long threadId, bool isPinned, CancellationToken cancellationToken)
    {
        var thread = await _applicationDbContext.Threads
            .TagWithCallSite()
            .FirstOrDefaultAsync(t => t.Id == threadId, cancellationToken);
        if (thread is null)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ErrorMessage = $"Thread with id {threadId} not found",
            };
        }

        thread.IsPinned = isPinned;
        thread.ModifiedAt = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        return default(Success);
    }

    public async Task<ThreadPatchResultModel> SetThreadCyclicAsync(long threadId, bool isCyclic, CancellationToken cancellationToken)
    {
        var thread = await _applicationDbContext.Threads
            .TagWithCallSite()
            .FirstOrDefaultAsync(t => t.Id == threadId, cancellationToken);
        if (thread is null)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ErrorMessage = $"Thread with id {threadId} not found",
            };
        }

        thread.IsCyclic = isCyclic;
        thread.ModifiedAt = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
        return default(Success);
    }

    public async Task<ThreadPatchResultModel> SetThreadClosedAsync(long threadId, bool isClosed, CancellationToken cancellationToken)
    {
        var thread = await _applicationDbContext.Threads
            .TagWithCallSite()
            .FirstOrDefaultAsync(t => t.Id == threadId, cancellationToken);
        if (thread is null)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ErrorMessage = $"Thread with id {threadId} not found",
            };
        }

        thread.IsClosed = isClosed;
        thread.ModifiedAt = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
        return default(Success);
    }

    public async Task<ThreadPatchResultModel> SetThreadDeletedAsync(long threadId, bool isDeleted, CancellationToken cancellationToken)
    {
        var thread = await _applicationDbContext.Threads
            .TagWithCallSite()
            .FirstOrDefaultAsync(t => t.Id == threadId, cancellationToken);
        if (thread is null)
        {
            return new DomainError
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ErrorMessage = $"Thread with id {threadId} not found",
            };
        }

        thread.IsDeleted = isDeleted;
        thread.ModifiedAt = _timeProvider.GetUtcNow().UtcDateTime;

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
        return default(Success);
    }
}
