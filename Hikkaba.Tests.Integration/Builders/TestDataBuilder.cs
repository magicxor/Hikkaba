using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Tests.Integration.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    private readonly ApplicationDbContext _dbContext;
    private int _guidCounter;

    public TestDataBuilder(IServiceScope scope)
    {
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        HashService = scope.ServiceProvider.GetRequiredService<IHashService>();
        TimeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    }

    public IHashService HashService { get; }

    public TimeProvider TimeProvider { get; }

    private Guid NextGuid(string seed)
    {
        return StableDataGen.GenerateDeterministicGuid($"{seed}_{_guidCounter++}");
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        await ApplyPendingUserRoleAssignmentsAsync(cancellationToken);
    }
}
