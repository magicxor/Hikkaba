using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.QueryableExtensions;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Microsoft.EntityFrameworkCore;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly TimeProvider _timeProvider;
    private readonly IAttachmentRepository _attachmentRepository;

    public PostRepository(
        ApplicationDbContext applicationDbContext,
        TimeProvider timeProvider,
        IAttachmentRepository attachmentRepository)
    {
        _applicationDbContext = applicationDbContext;
        _timeProvider = timeProvider;
        _attachmentRepository = attachmentRepository;
    }

    public async Task<IReadOnlyList<PostDetailsModel>> ListThreadPostsAsync(
        ThreadPostsFilter filter,
        CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Posts
            .Include(post => post.Thread)
            .ThenInclude(thread => thread.Category)
            .Include(post => post.Audios)
            .Include(post => post.Documents)
            .Include(post => post.Notices)
            .Include(post => post.Pictures)
            .Include(post => post.Videos)
            .Include(post => post.Replies)
            .Where(p => p.ThreadId == filter.ThreadId
                && (filter.IncludeDeleted || (!p.IsDeleted && !p.Thread.IsDeleted && !p.Thread.Category.IsDeleted)))
            .AsQueryable()
            .GetDetailsModel()
            .ApplyOrderBy(filter, post => post.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<PostDetailsModel>> SearchPostsPaginatedAsync(
        SearchPostsPagingFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _applicationDbContext.Posts
            .Include(post => post.Thread)
            .ThenInclude(thread => thread.Category)
            .Include(post => post.Audios)
            .Include(post => post.Documents)
            .Include(post => post.Notices)
            .Include(post => post.Pictures)
            .Include(post => post.Videos)
            .Include(post => post.Replies)
            .Where(post => !post.IsDeleted
                && !post.Thread.IsDeleted
                && !post.Thread.Category.IsDeleted
                && (post.MessageText.Contains(filter.SearchQuery)
                    || (post.Thread.Title.Contains(filter.SearchQuery)
                        && post == post.Thread.Posts.OrderBy(tp => tp.CreatedAt).FirstOrDefault())))
            .GetDetailsModel();

        var totalThreadCount = await query.CountAsync(cancellationToken);

        var data = await query
            .ApplyOrderByAndPaging(filter, x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return new PagedResult<PostDetailsModel>(data, filter, totalThreadCount);
    }

    public async Task<PagedResult<PostDetailsModel>> ListPostsPaginatedAsync(
        PostPagingFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _applicationDbContext.Posts
            .Include(post => post.Thread)
            .ThenInclude(thread => thread.Category)
            .Include(post => post.Audios)
            .Include(post => post.Documents)
            .Include(post => post.Notices)
            .Include(post => post.Pictures)
            .Include(post => post.Videos)
            .Include(post => post.Replies)
            .Where(post => !post.IsDeleted
                && !post.Thread.IsDeleted
                && !post.Thread.Category.IsDeleted)
            .GetDetailsModel();

        var totalThreadCount = await query.CountAsync(cancellationToken);

        var data = await query
            .ApplyOrderByAndPaging(filter, x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return new PagedResult<PostDetailsModel>(data, filter, totalThreadCount);
    }

    public async Task<long> CreatePostAsync(
        PostCreateRequestModel createRequestModel,
        FileAttachmentContainerCollection inputFiles,
        CancellationToken cancellationToken)
    {
        var attachments = _attachmentRepository.ToAttachmentEntities(inputFiles);

        var post = new Post
        {
            BlobContainerId = createRequestModel.BlobContainerId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = createRequestModel.IsSageEnabled,
            MessageText = createRequestModel.MessageText,
            MessageHtml = createRequestModel.MessageHtml,
            UserIpAddress = createRequestModel.UserIpAddress,
            UserAgent = createRequestModel.UserAgent,
            ThreadId = createRequestModel.ThreadId,
            Audios = attachments.Audios,
            Documents = attachments.Documents,
            Pictures = attachments.Pictures,
            Videos = attachments.Videos,
        };

        var postsToReply = await _applicationDbContext.Posts
            .Where(p => p.ThreadId == createRequestModel.ThreadId && createRequestModel.MentionedPosts.Contains(p.Id))
            .Select(p => new PostToReply
            {
                Post = p,
                Reply = post,
            })
            .ToListAsync(cancellationToken);

        _applicationDbContext.Posts.Add(post);
        _applicationDbContext.PostsToReplies.AddRange(postsToReply);

        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        return post.Id;
    }
}
