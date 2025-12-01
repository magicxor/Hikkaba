using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Thread;

internal sealed class CreateThreadTests : IntegrationTestBase
{
    private static ThreadCreateExtendedRequestModel CreateThreadRequest(
        string categoryAlias,
        string threadTitle,
        string messageText,
        string ipAddress = "127.0.0.1",
        string userAgent = "TestAgent")
    {
        var ip = IPAddress.Parse(ipAddress);

        return new ThreadCreateExtendedRequestModel
        {
            BaseModel = new ThreadCreateRequestModel
            {
                CategoryAlias = categoryAlias,
                ThreadTitle = threadTitle,
                BlobContainerId = Guid.NewGuid(),
                MessageHtml = messageText,
                MessageText = messageText,
                UserIpAddress = ip.GetAddressBytes(),
                UserAgent = userAgent,
                ClientInfo = new ClientInfoModel
                {
                    CountryIsoCode = "US",
                    BrowserType = "Chrome",
                    OsType = "Windows",
                },
            },
            ThreadSalt = Guid.NewGuid(),
            ThreadLocalUserHash = new byte[32],
            ClientInfo = new ClientInfoModel
            {
                CountryIsoCode = "US",
                BrowserType = "Chrome",
                OsType = "Windows",
            },
        };
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenValidRequest_CreatesThreadAndPost(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello world!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True, "Expected success result");
        var success = result.AsT0;
        Assert.That(success.ThreadId, Is.GreaterThan(0));
        Assert.That(success.PostId, Is.GreaterThan(0));
        Assert.That(success.DeletedBlobContainerIds, Is.Empty);

        // Verify thread was created
        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var createdThread = await dbContext.Threads
            .Include(t => t.Posts)
            .FirstOrDefaultAsync(t => t.Id == success.ThreadId, cancellationToken);

        Assert.That(createdThread, Is.Not.Null);
        Assert.That(createdThread!.Title, Is.EqualTo("New thread"));
        Assert.That(createdThread.Posts, Has.Count.EqualTo(1));
        var opPost = createdThread.Posts.First();
        Assert.That(opPost.MessageText, Is.EqualTo("Hello world!"));
        Assert.That(opPost.IsOriginalPost, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenCategoryNotFound_ReturnsDomainError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("nonexistent", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.IsT1, Is.True, "Expected error result");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo(404));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenCategoryIsDeleted_ReturnsDomainError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.IsT1, Is.True, "Expected error result");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo(404));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_UsesCategoryDefaultBumpLimit(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random", defaultBumpLimit: 250);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);
        var success = result.AsT0;

        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var thread = await dbContext.Threads.FirstAsync(t => t.Id == success.ThreadId, cancellationToken);
        Assert.That(thread.BumpLimit, Is.EqualTo(250));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenCategoryDefaultBumpLimitIsZero_UsesDefaultBumpLimit(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random", defaultBumpLimit: 0);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);
        var success = result.AsT0;

        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var thread = await dbContext.Threads.FirstAsync(t => t.Id == success.ThreadId, cancellationToken);
        Assert.That(thread.BumpLimit, Is.EqualTo(Defaults.DefaultBumpLimit));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_SetsCorrectClientInfo(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!", userAgent: "Mozilla/5.0 Firefox");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);
        var success = result.AsT0;

        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var post = await dbContext.Posts.FirstAsync(p => p.Id == success.PostId, cancellationToken);

        Assert.That(post.UserAgent, Is.EqualTo("Mozilla/5.0 Firefox"));
        Assert.That(post.CountryIsoCode, Is.EqualTo("US"));
        Assert.That(post.BrowserType, Is.EqualTo("Chrome"));
        Assert.That(post.OsType, Is.EqualTo("Windows"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_SetsLastBumpAtToCreatedAt(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);
        var success = result.AsT0;

        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var thread = await dbContext.Threads.FirstAsync(t => t.Id == success.ThreadId, cancellationToken);

        Assert.That(thread.LastBumpAt, Is.EqualTo(thread.CreatedAt));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_WhenMaxThreadCountReached_DeletesOldestThread(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.Scope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random");

        // Manually set MaxThreadCount to a small number for testing
        var category = builder.GetCategory("b");
        category.MaxThreadCount = 3;

        // Create 3 threads (at max capacity)
        builder
            .WithThreadAndOp("b", "Oldest thread", createdAt: utcNow.AddDays(-3), lastBumpAt: utcNow.AddDays(-3))
            .WithThreadAndOp("b", "Middle thread", createdAt: utcNow.AddDays(-2), lastBumpAt: utcNow.AddDays(-2))
            .WithThreadAndOp("b", "Newest thread", createdAt: utcNow.AddDays(-1), lastBumpAt: utcNow.AddDays(-1));

        await builder.SaveAsync(cancellationToken);

        var oldestThread = builder.GetThread("Oldest thread");

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "Brand new thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);
        var success = result.AsT0;
        Assert.That(success.DeletedBlobContainerIds, Has.Count.GreaterThan(0));

        // Verify oldest thread was deleted
        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedThread = await dbContext.Threads.FirstOrDefaultAsync(t => t.Id == oldestThread.Id, cancellationToken);
        Assert.That(deletedThread, Is.Null, "Oldest thread should be deleted");

        // Verify new thread was created
        var newThread = await dbContext.Threads.FirstOrDefaultAsync(t => t.Id == success.ThreadId, cancellationToken);
        Assert.That(newThread, Is.Not.Null);
        Assert.That(newThread!.Title, Is.EqualTo("Brand new thread"));

        // Verify thread count is at max
        var threadCount = await dbContext.Threads.CountAsync(t => t.CategoryId == category.Id, cancellationToken);
        Assert.That(threadCount, Is.EqualTo(3));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_SetsCorrectIpAddress(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var testIp = "192.168.1.100";
        var request = CreateThreadRequest("b", "New thread", "Hello!", ipAddress: testIp);

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);
        var success = result.AsT0;

        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var post = await dbContext.Posts.FirstAsync(p => p.Id == success.PostId, cancellationToken);

        Assert.That(post.UserIpAddress, Is.EqualTo(IPAddress.Parse(testIp).GetAddressBytes()));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task CreateThread_NewThreadHasDefaultFlags(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = CreateThreadRequest("b", "New thread", "Hello!");

        // Act
        await using var emptyAttachments = new FileAttachmentContainerCollection();
        var result = await repository.CreateThreadAsync(request, emptyAttachments, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);
        var success = result.AsT0;

        var dbContext = appScope.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var thread = await dbContext.Threads.FirstAsync(t => t.Id == success.ThreadId, cancellationToken);

        Assert.That(thread.IsPinned, Is.False);
        Assert.That(thread.IsClosed, Is.False);
        Assert.That(thread.IsCyclic, Is.False);
        Assert.That(thread.IsDeleted, Is.False);
    }
}
