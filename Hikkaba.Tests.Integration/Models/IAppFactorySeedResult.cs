using System;
using Microsoft.Extensions.DependencyInjection;

namespace Hikkaba.Tests.Integration.Models;

internal interface IAppFactorySeedResult : IDisposable
{
    IServiceScope Scope { get; set; }
    CustomAppFactory AppFactory { get; set; }
}
