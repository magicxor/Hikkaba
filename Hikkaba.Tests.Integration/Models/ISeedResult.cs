using System;
using Microsoft.Extensions.DependencyInjection;

namespace ReactReduxTodo.Tests.Integration.Models;

internal interface ISeedResult : IDisposable
{
    public IServiceScope Scope { get; set; }
}
