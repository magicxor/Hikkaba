using Hikkaba.Common.Constants;
using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Infrastructure.Models.Attachments;
using Hikkaba.Infrastructure.Models.Category;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Hikkaba.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Repositories.Implementations;

public class ThreadRepository : IThreadRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public ThreadRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<IReadOnlyList<long>> ListAllThreadIdsAsync(CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Threads
            .OrderByDescending(thread => thread.Id)
            .Select(thread => thread.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<ThreadPreviewSm>> ListThreadPreviewsPaginatedAsync(
        ThreadPreviewsFilter filter,
        CancellationToken cancellationToken)
    {
        var threadQuery = _applicationDbContext.Threads
            .Include(thread => thread.Category)
            .Include(thread => thread.Posts)
            .ThenInclude(post => post.Attachments)
            .Include(thread => thread.Posts)
            .ThenInclude(post => post.Replies)
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
            .Select(x => new
            {
                Thread = x,
                Category = x.Category,
                LastPostCreatedAt = x.Posts.Where(p => !p.IsSageEnabled && !p.IsDeleted).Max(p => p.CreatedAt),
                Posts = x.Posts
                    .Where(p => filter.IncludeDeleted || !p.IsDeleted)
                    .OrderBy(p => p.CreatedAt)
                    /* take the OP-post (the first post) */
                    .Take(1)
                    .Union(x.Posts
                        .Where(p => filter.IncludeDeleted || !p.IsDeleted)
                        .OrderByDescending(p => p.CreatedAt)
                        /* take the 3 last posts */
                        .Take(Defaults.LatestPostsCountInThreadPreview))
                    .ToList(),
                PostCount = x.Posts.Count(p => filter.IncludeDeleted || !p.IsDeleted),
            })
            .Where(thread => filter.IncludeDeleted || thread.PostCount > 0)
            .Select(g => new ThreadPreviewSm
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
                Posts = g.Posts.Select(post => new PostPreviewDto
                {
                    Index = 0,
                    Id = post.Id,
                    IsDeleted = post.IsDeleted,
                    CreatedAt = post.CreatedAt,
                    ModifiedAt = post.ModifiedAt,
                    IsSageEnabled = post.IsSageEnabled,
                    MessageHtml = post.MessageHtml,
                    UserIpAddress = post.UserIpAddress,
                    UserAgent = post.UserAgent,
                    Audio = post.Attachments.OfType<Audio>()
                        .Select(attachment => new AudioDto
                        {
                            Id = attachment.Id,
                            PostId = attachment.PostId,
                            ThreadId = post.ThreadId,
                            FileName = attachment.FileName,
                            FileExtension = attachment.FileExtension,
                            FileSize = attachment.FileSize,
                            FileHash = attachment.FileHash,
                        })
                        .ToList(),
                    Documents = post.Attachments.OfType<Document>()
                        .Select(attachment => new DocumentDto
                        {
                            Id = attachment.Id,
                            PostId = attachment.PostId,
                            ThreadId = post.ThreadId,
                            FileName = attachment.FileName,
                            FileExtension = attachment.FileExtension,
                            FileSize = attachment.FileSize,
                            FileHash = attachment.FileHash,
                        })
                        .ToList(),
                    Notices = post.Attachments.OfType<Notice>()
                        .Select(attachment => new NoticeDto
                        {
                            Id = attachment.Id,
                            PostId = attachment.PostId,
                            ThreadId = post.ThreadId,
                            Text = attachment.Text,
                        })
                        .ToList(),
                    Pictures = post.Attachments.OfType<Picture>()
                        .Select(attachment => new PictureDto
                        {
                            Id = attachment.Id,
                            PostId = attachment.PostId,
                            ThreadId = post.ThreadId,
                            FileName = attachment.FileName,
                            FileExtension = attachment.FileExtension,
                            FileSize = attachment.FileSize,
                            FileHash = attachment.FileHash,
                            Width = attachment.Width,
                            Height = attachment.Height,
                        })
                        .ToList(),
                    Video = post.Attachments.OfType<Video>()
                        .Select(attachment => new VideoDto
                        {
                            Id = attachment.Id,
                            PostId = attachment.PostId,
                            ThreadId = post.ThreadId,
                            FileName = attachment.FileName,
                            FileExtension = attachment.FileExtension,
                            FileSize = attachment.FileSize,
                            FileHash = attachment.FileHash,
                        })
                        .ToList(),
                    ThreadId = post.ThreadId,
                    ThreadShowThreadLocalUserHash = g.Thread.ShowThreadLocalUserHash,
                    CategoryAlias = g.Category.Alias,
                    CategoryId = g.Category.Id,
                    Replies = post.Replies
                        .Select(reply => reply.ReplyId)
                        .ToList(),
                })
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .ToList(),
            });

        var totalThreadCount = await resultQuery.CountAsync(cancellationToken);

        var data = await resultQuery
            .ApplyOrderByAndPaging(filter, x => x.LastPostCreatedAt, OrderByDirection.Desc)
            .ToListAsync(cancellationToken);

        return new PagedResult<ThreadPreviewSm>(data, filter, totalThreadCount);
    }
}
