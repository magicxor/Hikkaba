using MaxMind.Db;
using MaxMind.GeoIP2;

namespace Hikkaba.Application.Implementations;

public class GeoIpAsnReader : DatabaseReader
{
    public GeoIpAsnReader(string file, FileAccessMode mode = FileAccessMode.MemoryMapped) : base(file, mode)
    {
    }

    public GeoIpAsnReader(string file, IEnumerable<string> locales, FileAccessMode mode = FileAccessMode.MemoryMapped) : base(file, locales, mode)
    {
    }

    public GeoIpAsnReader(Stream stream) : base(stream)
    {
    }

    public GeoIpAsnReader(Stream stream, IEnumerable<string> locales) : base(stream, locales)
    {
    }
}
