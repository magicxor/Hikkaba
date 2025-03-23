using Hikkaba.Data.Context;
using Hikkaba.Data.Entities.Attachments;
using Hikkaba.Infrastructure.Models.Attachments;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Hikkaba.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Repositories.Implementations;

public class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public PostRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<IReadOnlyList<PostInfoRm>> ListThreadPostsAsync(
        ThreadPostsFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _applicationDbContext.Posts
            .Include(post => post.Thread)
            .ThenInclude(thread => thread.Category)
            .Include(post => post.Attachments)
            .Include(post => post.Replies)
            .Where(p => p.ThreadId == filter.ThreadId)
            .AsQueryable();

        query = filter.IncludeDeleted
            ? query.IgnoreQueryFilters()
            : query.Where(p => !p.IsDeleted && !p.Thread.IsDeleted && !p.Thread.Category.IsDeleted);

        return await query
            .ApplyOrderBy(filter, post => post.CreatedAt)
            .Select(post => new PostInfoRm
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
                    .Select(a => new AudioDto
                    {
                        Id = a.Id,
                        PostId = a.PostId,
                        ThreadId = post.ThreadId,
                        FileName = a.FileName,
                        FileExtension = a.FileExtension,
                        FileSize = a.FileSize,
                        FileHash = a.FileHash,
                    })
                    .ToList(),
                Documents = post.Attachments.OfType<Document>()
                    .Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        PostId = d.PostId,
                        ThreadId = post.ThreadId,
                        FileName = d.FileName,
                        FileExtension = d.FileExtension,
                        FileSize = d.FileSize,
                        FileHash = d.FileHash,
                    })
                    .ToList(),
                Notices = post.Attachments.OfType<Notice>()
                    .Select(n => new NoticeDto
                    {
                        Id = n.Id,
                        PostId = n.PostId,
                        ThreadId = post.ThreadId,
                        Text = n.Text,
                    })
                    .ToList(),
                Pictures = post.Attachments.OfType<Picture>()
                    .Select(p => new PictureDto
                    {
                        Id = p.Id,
                        PostId = p.PostId,
                        ThreadId = post.ThreadId,
                        FileName = p.FileName,
                        FileExtension = p.FileExtension,
                        FileSize = p.FileSize,
                        FileHash = p.FileHash,
                        Width = p.Width,
                        Height = p.Height,
                    })
                    .ToList(),
                Video = post.Attachments.OfType<Video>()
                    .Select(v => new VideoDto
                    {
                        Id = v.Id,
                        PostId = v.PostId,
                        ThreadId = post.ThreadId,
                        FileName = v.FileName,
                        FileExtension = v.FileExtension,
                        FileSize = v.FileSize,
                        FileHash = v.FileHash,
                    })
                    .ToList(),
                ThreadId = post.ThreadId,
                ThreadShowThreadLocalUserHash = post.Thread.ShowThreadLocalUserHash,
                CategoryAlias = post.Thread.Category.Alias,
                CategoryId = post.Thread.CategoryId,
                Replies = post.Replies
                    .Select(r => r.ReplyId)
                    .ToList(),
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<PostInfoRm>> SearchPostsPaginatedAsync(
        SearchPostsPagingFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _applicationDbContext.Posts
            .Include(post => post.Thread)
            .ThenInclude(thread => thread.Category)
            .Include(post => post.Attachments)
            .Include(post => post.Replies)
            .Where(post => !post.IsDeleted
                && !post.Thread.IsDeleted
                && !post.Thread.Category.IsDeleted
                && (post.MessageText.Contains(filter.SearchQuery)
                    || (post.Thread.Title.Contains(filter.SearchQuery)
                        && post == post.Thread.Posts.OrderBy(tp => tp.CreatedAt).FirstOrDefault())))
            .Select(post => new PostInfoRm
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
                    .Select(a => new AudioDto
                    {
                        Id = a.Id,
                        PostId = a.PostId,
                        ThreadId = post.ThreadId,
                        FileName = a.FileName,
                        FileExtension = a.FileExtension,
                        FileSize = a.FileSize,
                        FileHash = a.FileHash,
                    })
                    .ToList(),
                Documents = post.Attachments.OfType<Document>()
                    .Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        PostId = d.PostId,
                        ThreadId = post.ThreadId,
                        FileName = d.FileName,
                        FileExtension = d.FileExtension,
                        FileSize = d.FileSize,
                        FileHash = d.FileHash,
                    })
                    .ToList(),
                Notices = post.Attachments.OfType<Notice>()
                    .Select(n => new NoticeDto
                    {
                        Id = n.Id,
                        PostId = n.PostId,
                        ThreadId = post.ThreadId,
                        Text = n.Text,
                    })
                    .ToList(),
                Pictures = post.Attachments.OfType<Picture>()
                    .Select(p => new PictureDto
                    {
                        Id = p.Id,
                        PostId = p.PostId,
                        ThreadId = post.ThreadId,
                        FileName = p.FileName,
                        FileExtension = p.FileExtension,
                        FileSize = p.FileSize,
                        FileHash = p.FileHash,
                        Width = p.Width,
                        Height = p.Height,
                    })
                    .ToList(),
                Video = post.Attachments.OfType<Video>()
                    .Select(v => new VideoDto
                    {
                        Id = v.Id,
                        PostId = v.PostId,
                        ThreadId = post.ThreadId,
                        FileName = v.FileName,
                        FileExtension = v.FileExtension,
                        FileSize = v.FileSize,
                        FileHash = v.FileHash,
                    })
                    .ToList(),
                ThreadId = post.ThreadId,
                ThreadShowThreadLocalUserHash = post.Thread.ShowThreadLocalUserHash,
                CategoryAlias = post.Thread.Category.Alias,
                CategoryId = post.Thread.CategoryId,
                Replies = post.Replies
                    .Select(r => r.ReplyId)
                    .ToList(),
            });

        var totalThreadCount = await query.CountAsync(cancellationToken);

        var data = await query
            .ApplyOrderByAndPaging(filter, x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return new PagedResult<PostInfoRm>(data, filter, totalThreadCount);
    }

    public async Task<PagedResult<PostInfoRm>> ListPostsPaginatedAsync(
        PostPagingFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _applicationDbContext.Posts
            .Include(post => post.Thread)
            .ThenInclude(thread => thread.Category)
            .Include(post => post.Attachments)
            .Include(post => post.Replies)
            .Where(post => !post.IsDeleted
                && !post.Thread.IsDeleted
                && !post.Thread.Category.IsDeleted)
            .Select(post => new PostInfoRm
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
                    .Select(a => new AudioDto
                    {
                        Id = a.Id,
                        PostId = a.PostId,
                        ThreadId = post.ThreadId,
                        FileName = a.FileName,
                        FileExtension = a.FileExtension,
                        FileSize = a.FileSize,
                        FileHash = a.FileHash,
                    })
                    .ToList(),
                Documents = post.Attachments.OfType<Document>()
                    .Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        PostId = d.PostId,
                        ThreadId = post.ThreadId,
                        FileName = d.FileName,
                        FileExtension = d.FileExtension,
                        FileSize = d.FileSize,
                        FileHash = d.FileHash,
                    })
                    .ToList(),
                Notices = post.Attachments.OfType<Notice>()
                    .Select(n => new NoticeDto
                    {
                        Id = n.Id,
                        PostId = n.PostId,
                        ThreadId = post.ThreadId,
                        Text = n.Text,
                    })
                    .ToList(),
                Pictures = post.Attachments.OfType<Picture>()
                    .Select(p => new PictureDto
                    {
                        Id = p.Id,
                        PostId = p.PostId,
                        ThreadId = post.ThreadId,
                        FileName = p.FileName,
                        FileExtension = p.FileExtension,
                        FileSize = p.FileSize,
                        FileHash = p.FileHash,
                        Width = p.Width,
                        Height = p.Height,
                    })
                    .ToList(),
                Video = post.Attachments.OfType<Video>()
                    .Select(v => new VideoDto
                    {
                        Id = v.Id,
                        PostId = v.PostId,
                        ThreadId = post.ThreadId,
                        FileName = v.FileName,
                        FileExtension = v.FileExtension,
                        FileSize = v.FileSize,
                        FileHash = v.FileHash,
                    })
                    .ToList(),
                ThreadId = post.ThreadId,
                ThreadShowThreadLocalUserHash = post.Thread.ShowThreadLocalUserHash,
                CategoryAlias = post.Thread.Category.Alias,
                CategoryId = post.Thread.CategoryId,
                Replies = post.Replies
                    .Select(r => r.ReplyId)
                    .ToList(),
            });

        var totalThreadCount = await query.CountAsync(cancellationToken);

        var data = await query
            .ApplyOrderByAndPaging(filter, x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return new PagedResult<PostInfoRm>(data, filter, totalThreadCount);
    }
}
