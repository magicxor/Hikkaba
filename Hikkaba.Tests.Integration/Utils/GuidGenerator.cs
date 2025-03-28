using System;

namespace Hikkaba.Tests.Integration.Utils;

public class GuidGenerator
{
    private readonly Random _random;

    public GuidGenerator(int seed = 420)
    {
        _random = new Random(seed);
    }

    public Guid GenerateSeededGuid()
    {
        var guid = new byte[16];
        _random.NextBytes(guid);

        return new Guid(guid);
    }
}
