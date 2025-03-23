using System.Collections.Generic;
using System.IO;
using MaxMind.Db;
using MaxMind.GeoIP2;

namespace Hikkaba.Services.Implementations;

public class GeoIpReaderCountry : DatabaseReader
{
    public GeoIpReaderCountry(string file, FileAccessMode mode = FileAccessMode.MemoryMapped) : base(file, mode)
    {
    }

    public GeoIpReaderCountry(string file, IEnumerable<string> locales, FileAccessMode mode = FileAccessMode.MemoryMapped) : base(file, locales, mode)
    {
    }

    public GeoIpReaderCountry(Stream stream) : base(stream)
    {
    }

    public GeoIpReaderCountry(Stream stream, IEnumerable<string> locales) : base(stream, locales)
    {
    }
}
