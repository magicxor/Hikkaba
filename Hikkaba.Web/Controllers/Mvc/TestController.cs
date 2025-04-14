using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc;

[AllowAnonymous]
[Route("test")]
public sealed class TestController : Controller
{
    [HttpGet("status400")]
    public IActionResult Status400()
    {
        return new BadRequestResult();
    }

    [HttpGet("status404")]
    public IActionResult Status404()
    {
        return new NotFoundResult();
    }

    [HttpGet("status500")]
    public IActionResult Status500()
    {
        return new StatusCodeResult(500);
    }

    [HttpGet("exception")]
    public IActionResult Exception()
    {
        throw new InvalidOperationException("Test exception");
    }
}
