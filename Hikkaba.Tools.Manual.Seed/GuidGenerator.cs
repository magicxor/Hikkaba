using System.Diagnostics.CodeAnalysis;

namespace Hikkaba.Tools.Manual.Seed;

[ExcludeFromCodeCoverage]
internal sealed class GuidGenerator
{
    private readonly Random _random;

    public GuidGenerator(Random random)
    {
        _random = random;
    }

    public Guid GenerateSeededGuid()
    {
        var guid = new byte[16];
        _random.NextBytes(guid);

        return new Guid(guid);
    }
}
