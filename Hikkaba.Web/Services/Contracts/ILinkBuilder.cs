using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hikkaba.Web.Services.Contracts;

public interface ILinkBuilder
{
    public string? RouteUrl(
        string? routeName,
        object? values,
        PathString? pathBase = null,
        FragmentString fragment = default,
        LinkOptions? options = null);
}
