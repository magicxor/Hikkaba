using System;
using System.Diagnostics.CodeAnalysis;

namespace Hikkaba.Tests.Integration.Utils;

internal sealed class GuidGenerator
{
    private readonly Random _random;

    public GuidGenerator(int seed = 420)
    {
        _random = new Random(seed);
    }

    [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "This is a test utility, not production code.")]
    public Guid GenerateSeededGuid()
    {
        var guid = new byte[16];
        _random.NextBytes(guid);

        return new Guid(guid);
    }
}
