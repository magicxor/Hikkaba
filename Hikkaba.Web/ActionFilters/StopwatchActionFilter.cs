using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hikkaba.Web.ActionFilters;

public class StopwatchActionFilter : IActionFilter
{
    private readonly Stopwatch _sw = new();

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _sw.Start();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _sw.Stop();
        var ms = _sw.ElapsedMilliseconds;

        if (context.Result is ViewResult viewResult)
        {
            viewResult.ViewData["ExecutionTime"] = ms;
        }
    }
}
