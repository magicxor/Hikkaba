namespace Hikkaba.Tests.Integration.Models;

internal interface IAppFactoryScope : IAppScope
{
    CustomAppFactory AppFactory { get; set; }
}
