using Microsoft.AspNetCore.Http;

namespace Hikkaba.Tests.Unit.Mocks;

internal sealed class FakeHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; } = new DefaultHttpContext();
}
