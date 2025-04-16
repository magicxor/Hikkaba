namespace Hikkaba.Tests.Manual.LibManOptimizer;

internal sealed class LibManConfig
{
    // Property names match the JSON keys (case-insensitive by default with System.Text.Json)
    public string? Version { get; set; }
    public string? DefaultProvider { get; set; }
    public IReadOnlyList<LibraryInfo>? Libraries { get; set; }
}
