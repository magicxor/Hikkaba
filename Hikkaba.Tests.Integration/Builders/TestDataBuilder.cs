using System;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Contracts;
using Hikkaba.Data.Context;
using Hikkaba.Tests.Integration.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Builders;

internal sealed partial class TestDataBuilder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GuidGenerator _guidGenerator = new();

    public TestDataBuilder(IServiceScope scope)
    {
        _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        HashService = scope.ServiceProvider.GetRequiredService<IHashService>();
        TimeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    }

    public IHashService HashService { get; }

    public TimeProvider TimeProvider { get; }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        await ApplyPendingUserRoleAssignmentsAsync(cancellationToken);
    }
}
