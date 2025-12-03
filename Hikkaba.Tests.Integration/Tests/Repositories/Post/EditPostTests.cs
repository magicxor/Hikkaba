using System;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Data.Context;
using Hikkaba.Infrastructure.Models.Post;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Post;

internal sealed class EditPostTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditPost_WhenValidRequest_UpdatesPostContent(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original message", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var postId = builder.LastPostId;

        // Act
        await repository.EditPostAsync(new PostEditRequestModel
        {
            Id = postId,
            MessageText = "Updated message text",
            MessageHtml = "<p>Updated message html</p>",
        }, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedPost = await dbContext.Posts.FirstAsync(p => p.Id == postId, cancellationToken);

        Assert.That(updatedPost.MessageText, Is.EqualTo("Updated message text"));
        Assert.That(updatedPost.MessageHtml, Is.EqualTo("<p>Updated message html</p>"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditPost_DoesNotChangeOtherFields(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original message", "192.168.1.100", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var postId = builder.LastPostId;

        var originalPost = await dbContext.Posts.AsNoTracking().FirstAsync(p => p.Id == postId, cancellationToken);

        // Act
        await repository.EditPostAsync(new PostEditRequestModel
        {
            Id = postId,
            MessageText = "Changed text",
            MessageHtml = "Changed html",
        }, cancellationToken);

        // Assert
        var updatedPost = await dbContext.Posts.AsNoTracking().FirstAsync(p => p.Id == postId, cancellationToken);

        // Verify message was changed
        Assert.That(updatedPost.MessageText, Is.EqualTo("Changed text"));

        // Verify other fields remain unchanged
        Assert.That(updatedPost.UserIpAddress, Is.EqualTo(originalPost.UserIpAddress));
        Assert.That(updatedPost.UserAgent, Is.EqualTo(originalPost.UserAgent));
        Assert.That(updatedPost.IsOriginalPost, Is.EqualTo(originalPost.IsOriginalPost));
        Assert.That(updatedPost.IsSageEnabled, Is.EqualTo(originalPost.IsSageEnabled));
        Assert.That(updatedPost.ThreadId, Is.EqualTo(originalPost.ThreadId));
        Assert.That(updatedPost.BlobContainerId, Is.EqualTo(originalPost.BlobContainerId));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditPost_CanUpdateToEmptyMessage(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original message with content", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var postId = builder.LastPostId;

        // Act
        await repository.EditPostAsync(new PostEditRequestModel
        {
            Id = postId,
            MessageText = string.Empty,
            MessageHtml = string.Empty,
        }, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedPost = await dbContext.Posts.FirstAsync(p => p.Id == postId, cancellationToken);

        Assert.That(updatedPost.MessageText, Is.EqualTo(string.Empty));
        Assert.That(updatedPost.MessageHtml, Is.EqualTo(string.Empty));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditPost_WhenPostDoesNotExist_ThrowsException(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original message", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await repository.EditPostAsync(new PostEditRequestModel
            {
                Id = 999999, // Non-existent ID
                MessageText = "Updated",
                MessageHtml = "Updated",
            }, cancellationToken);
        });
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditPost_CanUpdateDeletedPost(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Deleted post message", isOriginalPost: true, isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var postId = builder.LastPostId;

        // Act
        await repository.EditPostAsync(new PostEditRequestModel
        {
            Id = postId,
            MessageText = "Updated deleted post",
            MessageHtml = "Updated deleted post",
        }, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedPost = await dbContext.Posts
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Id == postId, cancellationToken);

        Assert.That(updatedPost.MessageText, Is.EqualTo("Updated deleted post"));
        Assert.That(updatedPost.IsDeleted, Is.True);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task EditPost_PreservesSpecialCharacters(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new TestDataBuilder(appScope.ServiceScope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThread("Test thread")
            .WithPost("Original", isOriginalPost: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.ServiceScope.ServiceProvider.GetRequiredService<IPostRepository>();
        var postId = builder.LastPostId;

        var specialText = "Message with <special> & \"characters\" 'quotes' \n newlines";
        var specialHtml = "<p>HTML with &amp; entities &lt;tag&gt;</p>";

        // Act
        await repository.EditPostAsync(new PostEditRequestModel
        {
            Id = postId,
            MessageText = specialText,
            MessageHtml = specialHtml,
        }, cancellationToken);

        // Assert
        var dbContext = appScope.ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedPost = await dbContext.Posts.FirstAsync(p => p.Id == postId, cancellationToken);

        Assert.That(updatedPost.MessageText, Is.EqualTo(specialText));
        Assert.That(updatedPost.MessageHtml, Is.EqualTo(specialHtml));
    }
}
