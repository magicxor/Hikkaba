using System;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Models;

internal interface ISeedResult : IDisposable
{
    public IServiceScope Scope { get; set; }
}
