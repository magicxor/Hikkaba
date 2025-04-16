namespace Hikkaba.Tests.Manual.LibManOptimizer;

internal sealed class LibraryInfo
{
    public string? Library { get; set; }
    public string? Destination { get; set; }
    // Use List<string> so we can modify it
    public IReadOnlyList<string>? Files { get; set; }
}
