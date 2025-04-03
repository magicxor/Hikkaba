using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Hikkaba.Tests.Unit.Mocks;

public sealed class FakeActionContextAccessor : IActionContextAccessor
{
    public ActionContext? ActionContext { get; set; } = new(
        new HttpContextMock().SetupUrl("https://example.com").Mock.Object,
        new Microsoft.AspNetCore.Routing.RouteData(),
        new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
}
