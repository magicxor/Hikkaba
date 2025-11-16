using System;
using Hikkaba.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace ReactReduxTodo.Tests.Integration.Models;

internal sealed class SeedResult : ISeedResult, IAppFactorySeedResult
{
    public required IServiceScope Scope { get; set; }
    public required CustomAppFactory AppFactory { get; set; }

    public void Dispose()
    {
        Scope.Dispose();
        AppFactory.Dispose();
    }
}
