namespace Hikkaba.Infrastructure.Models.Category;

public class CategoryCreateRequestModel
{
    public required string Alias { get; set; }
    public required string Name { get; set; }
    public required bool IsHidden { get; set; }
    public required int DefaultBumpLimit { get; set; }
    public required bool ShowThreadLocalUserHash { get; set; }
    public required bool ShowUserAgent { get; set; }
    public required bool ShowCountry { get; set; }
    public required int MaxThreadCount { get; set; }
}
