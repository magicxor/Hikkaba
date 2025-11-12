using System;
using Hikkaba.Web.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hikkaba.Web.Services.Implementations;

public class LinkBuilder : ILinkBuilder
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;

    public LinkBuilder(
        IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator)
    {
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
    }

    public string? RouteUrl(
        string? routeName,
        object? values,
        PathString? pathBase = null,
        FragmentString fragment = default,
        LinkOptions? options = null)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");
        return _linkGenerator.GetPathByRouteValues(httpContext, routeName, values, pathBase, fragment, options);
    }
}
