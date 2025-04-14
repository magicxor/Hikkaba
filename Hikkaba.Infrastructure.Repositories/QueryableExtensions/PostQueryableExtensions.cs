using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Attachments.Concrete;
using Hikkaba.Infrastructure.Models.Post;
using Microsoft.EntityFrameworkCore;
using Thinktecture;

namespace Hikkaba.Infrastructure.Repositories.QueryableExtensions;

public static class PostQueryableExtensions
{
    public static IQueryable<PostDetailsModel> GetDetailsModel(this IQueryable<Post> query)
    {
        return query.Select(post => new PostDetailsModel
        {
            Index = EF.Functions.RowNumber(post.ThreadId,
                EF.Functions.OrderBy(post.CreatedAt)
                    .ThenBy(post.Id)),
            Id = post.Id,
            IsDeleted = post.IsDeleted,
            CreatedAt = post.CreatedAt,
            ModifiedAt = post.ModifiedAt,
            IsSageEnabled = post.IsSageEnabled,
            MessageHtml = post.MessageHtml,
            UserIpAddress = post.UserIpAddress,
            UserAgent = post.UserAgent,
            Audio = post.Audios
                .Select(a => new AudioModel
                {
                    Id = a.Id,
                    PostId = a.PostId,
                    ThreadId = post.ThreadId,
                    BlobId = a.BlobId,
                    BlobContainerId = post.BlobContainerId,
                    FileName = a.FileNameWithoutExtension,
                    FileExtension = a.FileExtension,
                    FileSize = a.FileSize,
                    FileContentType = a.FileContentType,
                    FileHash = a.FileHash,
                })
                .ToList(),
            Documents = post.Documents
                .Select(d => new DocumentModel
                {
                    Id = d.Id,
                    PostId = d.PostId,
                    ThreadId = post.ThreadId,
                    BlobId = d.BlobId,
                    BlobContainerId = post.BlobContainerId,
                    FileName = d.FileNameWithoutExtension,
                    FileExtension = d.FileExtension,
                    FileSize = d.FileSize,
                    FileContentType = d.FileContentType,
                    FileHash = d.FileHash,
                })
                .ToList(),
            Notices = post.Notices
                .Select(n => new NoticeModel
                {
                    Id = n.Id,
                    PostId = n.PostId,
                    ThreadId = post.ThreadId,
                    Text = n.Text,
                })
                .ToList(),
            Pictures = post.Pictures
                .Select(p => new PictureModel
                {
                    Id = p.Id,
                    PostId = p.PostId,
                    ThreadId = post.ThreadId,
                    BlobId = p.BlobId,
                    BlobContainerId = post.BlobContainerId,
                    FileName = p.FileNameWithoutExtension,
                    FileExtension = p.FileExtension,
                    FileSize = p.FileSize,
                    FileContentType = p.FileContentType,
                    FileHash = p.FileHash,
                    Width = p.Width,
                    Height = p.Height,
                })
                .ToList(),
            Video = post.Videos
                .Select(v => new VideoModel
                {
                    Id = v.Id,
                    PostId = v.PostId,
                    ThreadId = post.ThreadId,
                    BlobId = v.BlobId,
                    BlobContainerId = post.BlobContainerId,
                    FileName = v.FileNameWithoutExtension,
                    FileExtension = v.FileExtension,
                    FileSize = v.FileSize,
                    FileContentType = v.FileContentType,
                    FileHash = v.FileHash,
                })
                .ToList(),
            ThreadId = post.ThreadId,
            ShowThreadLocalUserHash = post.Thread.Category.ShowThreadLocalUserHash,
            ThreadLocalUserHash = post.ThreadLocalUserHash,
            ShowCountry = post.Thread.Category.ShowCountry,
            ShowOs = post.Thread.Category.ShowOs,
            ShowBrowser = post.Thread.Category.ShowBrowser,
            CategoryAlias = post.Thread.Category.Alias,
            CategoryId = post.Thread.CategoryId,
            Replies = post.RepliesToThisMentionedPost
                .Where(r => !r.Post.IsDeleted && !r.Reply.IsDeleted)
                .Select(r => r.ReplyId)
                .ToList(),
        });
    }
}
