using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hikkaba.Tests.Unit.Mocks;

internal sealed class FakeLinkGenerator : LinkGenerator
{
    private string? _expectedPath;
    private string? _expectedUri;

    public FakeLinkGenerator()
    {
    }

    public FakeLinkGenerator(string expectedPath, string expectedUri)
    {
        _expectedPath = expectedPath;
        _expectedUri = expectedUri;
    }

    public void SetExpectedPath(string expectedPath)
    {
        _expectedPath = expectedPath;
    }

    public void SetExpectedUri(string expectedUri)
    {
        _expectedUri = expectedUri;
    }

    public override string? GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary? ambientValues = null, PathString? pathBase = null, FragmentString fragment = new FragmentString(), LinkOptions? options = null)
    {
        return _expectedPath;
    }

    public override string? GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values, PathString pathBase = new PathString(), FragmentString fragment = new FragmentString(), LinkOptions? options = null)
    {
        return _expectedPath;
    }

    public override string? GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary? ambientValues = null, string? scheme = null, HostString? host = null, PathString? pathBase = null, FragmentString fragment = new FragmentString(), LinkOptions? options = null)
    {
        return _expectedUri;
    }

    public override string? GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string scheme, HostString host, PathString pathBase = new PathString(), FragmentString fragment = new FragmentString(), LinkOptions? options = null)
    {
        return _expectedUri;
    }
}
