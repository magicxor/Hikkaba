using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc;

[AllowAnonymous]
[Route("test")]
public sealed class TestController : Controller
{
    [HttpGet("status")]
    public IActionResult StatusCode()
    {
        return NotFound("blah blah blah");
    }

    [HttpGet("exception")]
    public IActionResult Exception()
    {
        throw new InvalidOperationException("Test exception");
    }
}
