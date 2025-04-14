using MaxMind.Db;
using MaxMind.GeoIP2;

namespace Hikkaba.Application.Implementations;

public sealed class GeoIpCountryReader : DatabaseReader
{
    public GeoIpCountryReader(string file, FileAccessMode mode = FileAccessMode.MemoryMapped)
        : base(file, mode)
    {
    }

    public GeoIpCountryReader(string file, IEnumerable<string> locales, FileAccessMode mode = FileAccessMode.MemoryMapped)
        : base(file, locales, mode)
    {
    }

    public GeoIpCountryReader(Stream stream)
        : base(stream)
    {
    }

    public GeoIpCountryReader(Stream stream, IEnumerable<string> locales)
        : base(stream, locales)
    {
    }
}
