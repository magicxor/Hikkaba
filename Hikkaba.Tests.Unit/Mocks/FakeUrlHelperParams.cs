using System;

namespace Hikkaba.Tests.Unit.Mocks;

internal sealed class FakeUrlHelperParams
{
    public string? Action { get; set; }

    /// <summary>
    /// routeName, values => routeUrl
    /// </summary>
    public required Func<string?, object?, string> RouteUrlFactory { get; set; }
}
