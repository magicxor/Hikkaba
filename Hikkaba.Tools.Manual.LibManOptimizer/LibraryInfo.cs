using JetBrains.Annotations;

namespace Hikkaba.Tools.Manual.LibManOptimizer;

internal sealed class LibraryInfo
{
    [UsedImplicitly]
    public string? Library { get; set; }

    [UsedImplicitly]
    public string? Destination { get; set; }

    public IReadOnlyList<string>? Files { get; set; }
}
