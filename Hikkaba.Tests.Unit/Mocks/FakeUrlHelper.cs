using System;
using System.Diagnostics.CodeAnalysis;
using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Hikkaba.Tests.Unit.Mocks;

internal sealed class FakeUrlHelper : IUrlHelper
{
    private const string BaseUri = "http://localhost";

    private readonly FakeUrlHelperParams _fakeUrlHelperParams;

    public FakeUrlHelper(FakeUrlHelperParams fakeUrlHelperParams)
    {
        _fakeUrlHelperParams = fakeUrlHelperParams;
    }

    public string? Action(UrlActionContext actionContext)
    {
        return _fakeUrlHelperParams.Action;
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
        return _fakeUrlHelperParams.RouteUrlFactory(routeContext.RouteName, routeContext.Values);
    }

    public string? Link(string? routeName, object? values)
    {
        var uri = new Uri(_fakeUrlHelperParams.RouteUrlFactory(routeName, values));
        var result = uri.IsAbsoluteUri
            ? uri
            : Uri.TryCreate(new Uri(BaseUri), uri, out var absoluteUri)
                ? absoluteUri
                : null;

        return result?.ToString();
    }

    public ActionContext ActionContext { get; } = new(
        new HttpContextMock().SetupUrl(BaseUri).Mock.Object,
        new Microsoft.AspNetCore.Routing.RouteData(),
        new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
}
