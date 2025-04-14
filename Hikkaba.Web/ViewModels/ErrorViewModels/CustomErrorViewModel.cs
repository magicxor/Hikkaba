using Microsoft.Extensions.Logging;

namespace Hikkaba.Web.ViewModels.ErrorViewModels;

public sealed class CustomErrorViewModel
{
    public required EventId EventId { get; set; }
    public required string ErrorMessage { get; set; }
    public required string? ReturnUrl { get; set; }
    public required int StatusCode { get; set; }
    public required string StatusCodeName { get; set; }
    public required string StatusCodeDescription { get; set; }
    public required string TraceId { get; set; }
}
