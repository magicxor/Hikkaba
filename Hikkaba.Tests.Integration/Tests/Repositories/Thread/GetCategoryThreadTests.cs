using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Infrastructure.Models.Thread;
using Hikkaba.Infrastructure.Repositories.Contracts;
using Hikkaba.Tests.Integration.Builders;
using Hikkaba.Tests.Integration.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Tests.Repositories.Thread;

internal sealed class GetCategoryThreadTests : IntegrationTestBase
{
    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategoryThread_WhenThreadExists_ReturnsThreadInfo(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("b", "Test thread");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Test thread");

        // Act
        var result = await repository.GetCategoryThreadAsync(new CategoryThreadFilter
        {
            CategoryAlias = "b",
            ThreadId = thread.Id,
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ThreadId, Is.EqualTo(thread.Id));
        Assert.That(result.CategoryAlias, Is.EqualTo("b"));
        Assert.That(result.CategoryName, Is.EqualTo("Random"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategoryThread_WhenThreadDoesNotExist_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();

        // Act
        var result = await repository.GetCategoryThreadAsync(new CategoryThreadFilter
        {
            CategoryAlias = "b",
            ThreadId = 999999,
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategoryThread_WhenCategoryAliasMismatch_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithCategory("a", "Anime")
            .WithThreadAndOp("b", "Thread in Random");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Thread in Random");

        // Act - request thread from category "a" but it's actually in "b"
        var result = await repository.GetCategoryThreadAsync(new CategoryThreadFilter
        {
            CategoryAlias = "a",
            ThreadId = thread.Id,
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategoryThread_WhenThreadIsDeleted_WithoutIncludeDeleted_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("b", "Deleted thread", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Deleted thread");

        // Act
        var result = await repository.GetCategoryThreadAsync(new CategoryThreadFilter
        {
            CategoryAlias = "b",
            ThreadId = thread.Id,
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategoryThread_WhenThreadIsDeleted_WithIncludeDeleted_ReturnsThread(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("b", "Deleted thread", isDeleted: true);

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Deleted thread");

        // Act
        var result = await repository.GetCategoryThreadAsync(new CategoryThreadFilter
        {
            CategoryAlias = "b",
            ThreadId = thread.Id,
            IncludeDeleted = true,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ThreadId, Is.EqualTo(thread.Id));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategoryThread_WhenCategoryIsDeleted_WithoutIncludeDeleted_ReturnsNull(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random", isDeleted: true)
            .WithThreadAndOp("b", "Thread in deleted category");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Thread in deleted category");

        // Act
        var result = await repository.GetCategoryThreadAsync(new CategoryThreadFilter
        {
            CategoryAlias = "b",
            ThreadId = thread.Id,
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategoryThread_WhenCategoryIsDeleted_WithIncludeDeleted_ReturnsThread(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random", isDeleted: true)
            .WithThreadAndOp("b", "Thread in deleted category");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var thread = builder.GetThread("Thread in deleted category");

        // Act
        var result = await repository.GetCategoryThreadAsync(new CategoryThreadFilter
        {
            CategoryAlias = "b",
            ThreadId = thread.Id,
            IncludeDeleted = true,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ThreadId, Is.EqualTo(thread.Id));
        Assert.That(result.CategoryAlias, Is.EqualTo("b"));
    }

    [CancelAfter(TestDefaults.TestTimeout)]
    [Test]
    public async Task GetCategoryThread_WhenMultipleThreadsExist_ReturnsCorrectOne(
        CancellationToken cancellationToken)
    {
        // Arrange
        using var appScope = await CreateAppScopeAsync(cancellationToken);
        var builder = new ThreadTestDataBuilder(appScope.Scope)
            .WithDefaultAdmin()
            .WithCategory("b", "Random")
            .WithThreadAndOp("b", "Thread 1")
            .WithThreadAndOp("b", "Thread 2")
            .WithThreadAndOp("b", "Thread 3");

        await builder.SaveAsync(cancellationToken);

        var repository = appScope.Scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var targetThread = builder.GetThread("Thread 2");

        // Act
        var result = await repository.GetCategoryThreadAsync(new CategoryThreadFilter
        {
            CategoryAlias = "b",
            ThreadId = targetThread.Id,
            IncludeDeleted = false,
        }, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ThreadId, Is.EqualTo(targetThread.Id));
    }
}
