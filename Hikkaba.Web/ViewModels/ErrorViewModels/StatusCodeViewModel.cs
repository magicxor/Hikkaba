namespace Hikkaba.Web.ViewModels.ErrorViewModels;

public class StatusCodeViewModel
{
    public required int StatusCode { get; set; }
    public required string StatusCodeName { get; set; }
    public required string StatusCodeDescription { get; set; }
    public required string TraceId { get; set; }
}
