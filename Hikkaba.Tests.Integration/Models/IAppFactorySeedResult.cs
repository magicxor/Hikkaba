using System;
using Hikkaba.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace ReactReduxTodo.Tests.Integration.Models;

internal interface IAppFactorySeedResult : IDisposable
{
    IServiceScope Scope { get; set; }
    CustomAppFactory AppFactory { get; set; }
}
