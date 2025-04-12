using Hikkaba.Data.Context;
using Hikkaba.Data.Entities;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Infrastructure.Repositories.QueryableExtensions;
using Hikkaba.Infrastructure.Repositories.Telemetry;
using Hikkaba.Paging.Extensions;
using Hikkaba.Paging.Models;
using Hikkaba.Shared.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Infrastructure.Repositories.Implementations;

public sealed class PostRepository : IPostRepository
{
    private readonly ILogger<PostRepository> _logger;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly TimeProvider _timeProvider;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IUserContext _userContext;

    public PostRepository(
        ILogger<PostRepository> logger,
        ApplicationDbContext applicationDbContext,
        TimeProvider timeProvider,
        IAttachmentRepository attachmentRepository,
        IUserContext userContext)
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
        _timeProvider = timeProvider;
        _attachmentRepository = attachmentRepository;
        _userContext = userContext;
    }

    public async Task<IReadOnlyList<PostDetailsModel>> ListThreadPostsAsync(
        ThreadPostsFilter filter,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.PostSource.StartActivity();

        return await _applicationDbContext.Posts
            .TagWithCallSite()
            .Include(post => post.Thread)
            .ThenInclude(thread => thread.Category)
            .Include(post => post.Audios)
            .Include(post => post.Documents)
            .Include(post => post.Notices)
            .Include(post => post.Pictures)
            .Include(post => post.Videos)
            .Include(post => post.RepliesToThisMentionedPost)
            .Where(p => p.ThreadId == filter.ThreadId
                        && (filter.PostId == null || p.Id == filter.PostId)
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
        using var activity = RepositoriesTelemetry.PostSource.StartActivity();

        var query = _applicationDbContext.Posts
            .TagWithCallSite()
            .Include(post => post.Thread)
            .ThenInclude(thread => thread.Category)
            .Include(post => post.Audios)
            .Include(post => post.Documents)
            .Include(post => post.Notices)
            .Include(post => post.Pictures)
            .Include(post => post.Videos)
            .Include(post => post.RepliesToThisMentionedPost)
            .Where(post => !post.IsDeleted
                           && !post.Thread.IsDeleted
                           && !post.Thread.Category.IsDeleted
                           && EF.Functions.Contains(post.MessageText, filter.SearchQuery))
            .GetDetailsModel();

        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query
            .ApplyOrderByAndPaging(filter, x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return new PagedResult<PostDetailsModel>(data, filter, totalCount);
    }

    public async Task<PagedResult<PostDetailsModel>> ListPostsPaginatedAsync(
        PostPagingFilter filter,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.PostSource.StartActivity();

        var query = _applicationDbContext.Posts
            .TagWithCallSite()
            .Include(post => post.Thread)
            .ThenInclude(thread => thread.Category)
            .Include(post => post.Audios)
            .Include(post => post.Documents)
            .Include(post => post.Notices)
            .Include(post => post.Pictures)
            .Include(post => post.Videos)
            .Include(post => post.RepliesToThisMentionedPost)
            .Where(post => filter.IncludeDeleted
                           || (!post.IsDeleted
                               && !post.Thread.IsDeleted
                               && !post.Thread.Category.IsDeleted))
            .Where(post => filter.IncludeHidden || !post.Thread.Category.IsHidden)
            .GetDetailsModel();

        int? totalCount = filter.IncludeTotalCount ? await query.CountAsync(cancellationToken) : null;

        var data = await query
            .ApplyOrderByAndPaging(filter, x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return new PagedResult<PostDetailsModel>(data, filter, totalCount);
    }

    public async Task<PostCreateResultSuccessModel> CreatePostAsync(
        PostCreateExtendedRequestModel requestModel,
        FileAttachmentContainerCollection inputFiles,
        CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.PostSource.StartActivity();
        _logger.LogInformation("Creating post in category {CategoryAlias}. ThreadId: {ThreadId}, Attachment count: {AttachmentCount}, BlobContainerId: {BlobContainerId}",
            requestModel.BaseModel.CategoryAlias,
            requestModel.BaseModel.ThreadId,
            inputFiles.Count,
            requestModel.BaseModel.BlobContainerId);

        var attachments = _attachmentRepository.ToAttachmentEntities(inputFiles);

        var deletedBlobContainerIds = new List<Guid>();

        if (requestModel is { IsCyclic: true, BumpLimit: > 0, PostCount: > 0 }
            && requestModel.PostCount >= requestModel.BumpLimit)
        {
            var postsToBeDeleted = await _applicationDbContext.Posts
                .TagWithCallSite()
                .Where(p => p.ThreadId == requestModel.BaseModel.ThreadId)
                .OrderBy(p => p.CreatedAt)
                .ThenBy(p => p.Id)
                .Skip(1) /* skip the original post */
                .Take(1) /* delete the oldest post */
                .ToListAsync(cancellationToken);

            deletedBlobContainerIds = postsToBeDeleted.ConvertAll(p => p.BlobContainerId);

            _logger.LogInformation("Deleting old post(s) in cyclic thread. ThreadId: {ThreadId}, PostCount: {PostCount}, BumpLimit: {BumpLimit}, Blob containers to be deleted: {BlobContainerCount}",
                requestModel.BaseModel.ThreadId,
                requestModel.PostCount,
                requestModel.BumpLimit,
                deletedBlobContainerIds.Count);

            _applicationDbContext.Posts.RemoveRange(postsToBeDeleted);
        }

        var post = new Post
        {
            BlobContainerId = requestModel.BaseModel.BlobContainerId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            IsSageEnabled = requestModel.BaseModel.IsSageEnabled,
            MessageText = requestModel.BaseModel.MessageText,
            MessageHtml = requestModel.BaseModel.MessageHtml,
            UserIpAddress = requestModel.BaseModel.UserIpAddress,
            UserAgent = requestModel.BaseModel.UserAgent,
            ThreadLocalUserHash = requestModel.ThreadLocalUserHash,
            ThreadId = requestModel.BaseModel.ThreadId,
            Audios = attachments.Audios,
            Documents = attachments.Documents,
            Pictures = attachments.Pictures,
            Videos = attachments.Videos,
        };

        var postsToReply = await _applicationDbContext.Posts
            .TagWithCallSite()
            .Where(p => p.ThreadId == requestModel.BaseModel.ThreadId
                        && requestModel.BaseModel.MentionedPosts.Contains(p.Id))
            .Select(p => new PostToReply
            {
                Post = p,
                Reply = post,
            })
            .ToListAsync(cancellationToken);

        _applicationDbContext.Posts.Add(post);
        _applicationDbContext.PostsToReplies.AddRange(postsToReply);

        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        return new PostCreateResultSuccessModel
        {
            PostId = post.Id,
            DeletedBlobContainerIds = deletedBlobContainerIds,
        };
    }

    public async Task SetPostDeletedAsync(long postId, bool isDeleted, CancellationToken cancellationToken)
    {
        using var activity = RepositoriesTelemetry.PostSource.StartActivity();

        var post = await _applicationDbContext.Posts
            .TagWithCallSite()
            .FirstAsync(p => p.Id == postId, cancellationToken);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var user = _userContext.GetUser();

        post.IsDeleted = isDeleted;
        post.ModifiedAt = utcNow;
        post.ModifiedById = user?.Id;

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }
}
