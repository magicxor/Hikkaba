namespace Hikkaba.Infrastructure.Models.Category;

public sealed class CategoryEditRequestModel
{
    public required int Id { get; set; }

    public required string Alias { get; set; }

    public required string Name { get; set; }

    public required bool IsHidden { get; set; }

    public required int DefaultBumpLimit { get; set; }

    public required bool ShowThreadLocalUserHash { get; set; }

    public required bool ShowCountry { get; set; }

    public required bool ShowOs { get; init; }

    public required bool ShowBrowser { get; init; }

    public required int MaxThreadCount { get; set; }
}
