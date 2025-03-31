using System.Net;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Attachments.Concrete;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.QueryableExtensions;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Thinktecture;
using Thread = Hikkaba.Data.Entities.Thread;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public class ThreadRepository : IThreadRepository
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly TimeProvider _timeProvider;
    private readonly IAttachmentRepository _attachmentRepository;

    public ThreadRepository(
        ApplicationDbContext applicationDbContext,
        TimeProvider timeProvider,
        IAttachmentRepository attachmentRepository)
    {
        _applicationDbContext = applicationDbContext;
        _timeProvider = timeProvider;
        _attachmentRepository = attachmentRepository;
    }

    public async Task<ThreadDetailsRequestModel?> GetThreadDetailsAsync(long threadId, bool includeDeleted, CancellationToken cancellationToken)
    {
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
            BumpLimit = thread.BumpLimit,
            ShowThreadLocalUserHash = thread.ShowThreadLocalUserHash,
            CategoryId = thread.Category.Id,
            CategoryAlias = thread.Category.Alias,
            CategoryName = thread.Category.Name,
            PostCount = posts.Count,
            Posts = posts,
        };
    }

    public async Task<IReadOnlyList<long>> ListAllThreadIdsAsync(CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Threads
            .TagWithCallSite()
            .OrderByDescending(thread => thread.Id)
            .Select(thread => thread.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<ThreadPreviewModel>> ListThreadPreviewsPaginatedAsync(
        ThreadPreviewFilter filter,
        CancellationToken cancellationToken)
    {
        var threadQuery = _applicationDbContext.Threads
            .TagWithCallSite()
            .Include(thread => thread.Category)
            .Include(thread => thread.Posts)
            .ThenInclude(post => post.Audios)
            .Include(thread => thread.Posts)
            .ThenInclude(post => post.Documents)
            .Include(thread => thread.Posts)
            .ThenInclude(post => post.Notices)
            .Include(thread => thread.Posts)
            .ThenInclude(post => post.Pictures)
            .Include(thread => thread.Posts)
            .ThenInclude(post => post.Videos)
            .Include(thread => thread.Posts)
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
                BumpLimit = g.Thread.BumpLimit,
                ShowThreadLocalUserHash = g.Thread.ShowThreadLocalUserHash,
                CategoryId = g.Category.Id,
                CategoryAlias = g.Category.Alias,
                CategoryName = g.Category.Name,
                PostCount = g.PostCount,
                Posts = g.Posts.Select(post => new PostDetailsModel
                {
                    Index = EF.Functions.RowNumber(EF.Functions.OrderByDescending(post.CreatedAt).ThenByDescending(post.Id)),
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
                            FileHash = attachment.FileHash,
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
                            FileHash = attachment.FileHash,
                            Width = attachment.Width,
                            Height = attachment.Height,
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
                            FileHash = attachment.FileHash,
                        })
                        .ToList(),
                    ThreadId = post.ThreadId,
                    ThreadShowThreadLocalUserHash = g.Thread.ShowThreadLocalUserHash,
                    CategoryAlias = g.Category.Alias,
                    CategoryId = g.Category.Id,
                    Replies = post.RepliesToThisMentionedPost
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
        ThreadCreateRequestModel createRequestModel,
        FileAttachmentContainerCollection inputFiles,
        CancellationToken cancellationToken)
    {
        var attachments = _attachmentRepository.ToAttachmentEntities(inputFiles);

        var category = await _applicationDbContext.Categories
            .TagWithCallSite()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(x => x.Alias == createRequestModel.CategoryAlias && !x.IsDeleted, cancellationToken);

        if (category is null)
        {
            throw new HikkabaHttpResponseException(HttpStatusCode.NotFound, $"Category with alias {createRequestModel.CategoryAlias} not found");
        }

        var post = new Post
        {
            BlobContainerId = createRequestModel.BlobContainerId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = createRequestModel.IsSageEnabled,
            MessageText = createRequestModel.MessageText,
            MessageHtml = createRequestModel.MessageHtml,
            UserIpAddress = createRequestModel.UserIpAddress,
            UserAgent = createRequestModel.UserAgent,
            Audios = attachments.Audios,
            Documents = attachments.Documents,
            Pictures = attachments.Pictures,
            Videos = attachments.Videos,
        };

        var thread = new Thread
        {
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            Title = createRequestModel.ThreadTitle,
            ShowThreadLocalUserHash = false,
            CategoryId = category.Id,
            Posts = [post],
        };

        _applicationDbContext.Threads.Add(thread);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        return new ThreadPostCreateResultModel
        {
            ThreadId = thread.Id,
            PostId = post.Id,
        };
    }
}
