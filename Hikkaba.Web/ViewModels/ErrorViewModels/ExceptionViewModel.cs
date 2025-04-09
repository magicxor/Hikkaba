namespace Hikkaba.Web.ViewModels.ErrorViewModels;

public class ExceptionViewModel
{
    public required string ExceptionName { get; set; }
    public required int StatusCode { get; set; }
    public required string StatusCodeName { get; set; }
    public required string StatusCodeDescription { get; set; }
    public required string TraceId { get; set; }
}
