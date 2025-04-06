using System.Diagnostics.CodeAnalysis;
using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Hikkaba.Tests.Unit.Mocks;

internal sealed class FakeUrlHelper : IUrlHelper
{
    private readonly string _action;

    public FakeUrlHelper(string action)
    {
        _action = action;
    }

    public string? Action(UrlActionContext actionContext)
    {
        return _action;
    }

    [return: NotNullIfNotNull(nameof(contentPath))]
    public string? Content(string? contentPath)
    {
        return "FakeContent";
    }

    public bool IsLocalUrl([NotNullWhen(true)] string? url)
    {
        return true;
    }

    public string? RouteUrl(UrlRouteContext routeContext)
    {
        return "FakeRouteUrl";
    }

    public string? Link(string? routeName, object? values)
    {
        return "FakeLink";
    }

    public ActionContext ActionContext { get; } = new ActionContext(
        new HttpContextMock().SetupUrl("https://example.com").Mock.Object,
        new Microsoft.AspNetCore.Routing.RouteData(),
        new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
}
