using System;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Thread;

internal sealed class EditThreadTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditThread_WhenValidRequest_UpdatesThread(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Original title");

        await builder.SaveAsync(cancellationToken);

        var thread = builder.GetThread("Original title");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = new ThreadEditRequestModel
        {
            Id = thread.Id,
            Title = "Updated title",
            BumpLimit = 500,
        };

        // Act
        var result = await repository.EditThreadAsync(request, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True, "Expected success result");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == thread.Id, cancellationToken);

        Assert.That(updatedThread.Title, Is.EqualTo("Updated title"));
        Assert.That(updatedThread.BumpLimit, Is.EqualTo(500));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditThread_WhenThreadNotFound_ReturnsDomainError(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Some thread");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = new ThreadEditRequestModel
        {
            Id = 999999,
            Title = "New title",
            BumpLimit = 300,
        };

        // Act
        var result = await repository.EditThreadAsync(request, cancellationToken);

        // Assert
        Assert.That(result.IsT1, Is.True, "Expected error result");
        var error = result.AsT1;
        Assert.That(error.StatusCode, Is.EqualTo(404));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditThread_WhenThreadIsDeleted_StillUpdatesThread(
        CancellationToken cancellationToken)
    {
        // Arrange
        // Note: EditThreadAsync does not check IsDeleted flag, so it allows editing deleted threads
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Deleted thread", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var thread = builder.GetThread("Deleted thread");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = new ThreadEditRequestModel
        {
            Id = thread.Id,
            Title = "New title",
            BumpLimit = 300,
        };

        // Act
        var result = await repository.EditThreadAsync(request, cancellationToken);

        // Assert
        // EditThreadAsync does not check IsDeleted, so it succeeds
        Assert.That(result.IsT0, Is.True, "Expected success result");

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == thread.Id, cancellationToken);

        Assert.That(updatedThread.Title, Is.EqualTo("New title"));
        Assert.That(updatedThread.BumpLimit, Is.EqualTo(300));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditThread_OnlyUpdatesTitle(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Original title");

        await builder.SaveAsync(cancellationToken);

        var thread = builder.GetThread("Original title");
        var originalBumpLimit = thread.BumpLimit;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = new ThreadEditRequestModel
        {
            Id = thread.Id,
            Title = "Updated title",
            BumpLimit = originalBumpLimit, // Keep the same
        };

        // Act
        var result = await repository.EditThreadAsync(request, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == thread.Id, cancellationToken);

        Assert.That(updatedThread.Title, Is.EqualTo("Updated title"));
        Assert.That(updatedThread.BumpLimit, Is.EqualTo(originalBumpLimit));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditThread_OnlyUpdatesBumpLimit(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Original title");

        await builder.SaveAsync(cancellationToken);

        var thread = builder.GetThread("Original title");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = new ThreadEditRequestModel
        {
            Id = thread.Id,
            Title = thread.Title!, // Keep the same
            BumpLimit = 999,
        };

        // Act
        var result = await repository.EditThreadAsync(request, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == thread.Id, cancellationToken);

        Assert.That(updatedThread.Title, Is.EqualTo("Original title"));
        Assert.That(updatedThread.BumpLimit, Is.EqualTo(999));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditThread_PreservesOtherThreadProperties(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var timeProvider = appScope.ServiceScope.ServiceProvider.GetRequiredService<TimeProvider>();
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Test thread", createdAt: utcNow.AddDays(-1));

        await builder.SaveAsync(cancellationToken);

        var thread = builder.GetThread("Test thread");
        var originalCreatedAt = thread.CreatedAt;
        var originalLastBumpAt = thread.LastBumpAt;
        var originalSalt = thread.Salt;
        var originalCategoryId = thread.CategoryId;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = new ThreadEditRequestModel
        {
            Id = thread.Id,
            Title = "Modified title",
            BumpLimit = 777,
        };

        // Act
        var result = await repository.EditThreadAsync(request, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == thread.Id, cancellationToken);

        Assert.That(updatedThread.Title, Is.EqualTo("Modified title"));
        Assert.That(updatedThread.BumpLimit, Is.EqualTo(777));

        // Verify unchanged properties
        Assert.That(updatedThread.CreatedAt, Is.EqualTo(originalCreatedAt));
        Assert.That(updatedThread.LastBumpAt, Is.EqualTo(originalLastBumpAt));
        Assert.That(updatedThread.Salt, Is.EqualTo(originalSalt));
        Assert.That(updatedThread.CategoryId, Is.EqualTo(originalCategoryId));
        Assert.That(updatedThread.IsPinned, Is.EqualTo(thread.IsPinned));
        Assert.That(updatedThread.IsClosed, Is.EqualTo(thread.IsClosed));
        Assert.That(updatedThread.IsCyclic, Is.EqualTo(thread.IsCyclic));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditThread_WhenEmptyTitle_UpdatesToEmptyTitle(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Original title");

        await builder.SaveAsync(cancellationToken);

        var thread = builder.GetThread("Original title");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = new ThreadEditRequestModel
        {
            Id = thread.Id,
            Title = string.Empty,
            BumpLimit = 300,
        };

        // Act
        var result = await repository.EditThreadAsync(request, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == thread.Id, cancellationToken);

        Assert.That(updatedThread.Title, Is.EqualTo(string.Empty));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditThread_CanSetBumpLimitToZero(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Test thread");

        await builder.SaveAsync(cancellationToken);

        var thread = builder.GetThread("Test thread");

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = new ThreadEditRequestModel
        {
            Id = thread.Id,
            Title = thread.Title!,
            BumpLimit = 0,
        };

        // Act
        var result = await repository.EditThreadAsync(request, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedThread = await dbContext.Threads.FirstAsync(t => t.Id == thread.Id, cancellationToken);

        Assert.That(updatedThread.BumpLimit, Is.EqualTo(0));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditThread_DoesNotAffectOtherThreads(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("Thread to edit")
            .WithThreadAndOp("Other thread");

        await builder.SaveAsync(cancellationToken);

        var threadToEdit = builder.GetThread("Thread to edit");
        var otherThread = builder.GetThread("Other thread");
        var otherThreadOriginalTitle = otherThread.Title;
        var otherThreadOriginalBumpLimit = otherThread.BumpLimit;

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var request = new ThreadEditRequestModel
        {
            Id = threadToEdit.Id,
            Title = "Modified title",
            BumpLimit = 999,
        };

        // Act
        var result = await repository.EditThreadAsync(request, cancellationToken);

        // Assert
        Assert.That(result.IsT0, Is.True);

        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var unchangedThread = await dbContext.Threads.FirstAsync(t => t.Id == otherThread.Id, cancellationToken);

        Assert.That(unchangedThread.Title, Is.EqualTo(otherThreadOriginalTitle));
        Assert.That(unchangedThread.BumpLimit, Is.EqualTo(otherThreadOriginalBumpLimit));
    }
}
