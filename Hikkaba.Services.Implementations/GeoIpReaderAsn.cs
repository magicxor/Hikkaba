using MaxMind.Db;
using MaxMind.GeoIP2;

namespace Hikkaba.Services.Implementations;

public class GeoIpReaderAsn : DatabaseReader
{
    public GeoIpReaderAsn(string file, FileAccessMode mode = FileAccessMode.MemoryMapped) : base(file, mode)
    {
    }

    public GeoIpReaderAsn(string file, IEnumerable<string> locales, FileAccessMode mode = FileAccessMode.MemoryMapped) : base(file, locales, mode)
    {
    }

    public GeoIpReaderAsn(Stream stream) : base(stream)
    {
    }

    public GeoIpReaderAsn(Stream stream, IEnumerable<string> locales) : base(stream, locales)
    {
    }
}
