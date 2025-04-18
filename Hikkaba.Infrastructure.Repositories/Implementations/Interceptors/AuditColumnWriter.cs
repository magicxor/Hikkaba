using System.Diagnostics.CodeAnalysis;
using Hikkaba.Data.Contracts;
using Hikkaba.Shared.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Hikkaba.Infrastructure.Repositories.Implementations.Interceptors;

public class AuditColumnWriter : SaveChangesInterceptor, IAuditColumnWriter
{
    private readonly ILogger<AuditColumnWriter> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;

    public AuditColumnWriter(
        ILogger<AuditColumnWriter> logger,
        TimeProvider timeProvider,
        IUserContext userContext)
    {
        _logger = logger;
        _timeProvider = timeProvider;
        _userContext = userContext;
    }

    [SuppressMessage("Major Bug", "S2583:Conditionally executed code should be reachable", Justification = "False positive.")]
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var savedEntries = eventData.Context.ChangeTracker
            .Entries()
            .Where(entry => entry.State != EntityState.Detached
                            && entry.State != EntityState.Unchanged)
            .ToList()
            .AsReadOnly();

        if (savedEntries.Count == 0)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        try
        {
            var currentUser = _userContext.GetUser();

            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                if (entry.State is EntityState.Added && entry.Entity is IHasCreatedAt hasCreatedAt)
                {
                    hasCreatedAt.CreatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                }
                if (entry.State is EntityState.Added && entry.Entity is IHasCreatedById createdById && currentUser is not null)
                {
                    createdById.CreatedById = currentUser.Id;
                }
                if (entry.State is EntityState.Modified && entry.Entity is IHasModifiedAt hasUpdatedAt)
                {
                    hasUpdatedAt.ModifiedAt = _timeProvider.GetUtcNow().UtcDateTime;
                }
                if (entry.State is EntityState.Modified && entry.Entity is IHasModifiedById modifiedById && currentUser is not null)
                {
                    modifiedById.ModifiedById = currentUser.Id;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while remembering the state of the entities");
        }

        // no need to call SaveChanges here, as it's about to happen
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
