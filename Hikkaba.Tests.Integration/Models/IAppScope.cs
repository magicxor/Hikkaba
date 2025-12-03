using System;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Models;

internal interface IAppScope : IDisposable
{
    public IServiceScope ServiceScope { get; set; }
}
