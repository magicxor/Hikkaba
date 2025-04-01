using System.Text;
using Blake3;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Telemetry;

namespace Hikkaba.Application.Implementations;

public class HashService : IHashService
{
    public byte[] GetHashBytes(Stream inputStream)
    {
        using var activity = ApplicationTelemetry.HashServiceSource.StartActivity();
        using var blake3Stream = new Blake3Stream(inputStream);
        return blake3Stream.ComputeHash().AsSpan().ToArray();
    }

    public byte[] GetHashBytes(string input)
    {
        using var activity = ApplicationTelemetry.HashServiceSource.StartActivity();
        return Hasher.Hash(Encoding.UTF8.GetBytes(input)).AsSpan().ToArray();
    }

    public string GetHashHex(Stream inputStream)
    {
        using var activity = ApplicationTelemetry.HashServiceSource.StartActivity();
        return Convert.ToHexStringLower(GetHashBytes(inputStream));
    }

    public string GetHashHex(string input)
    {
        using var activity = ApplicationTelemetry.HashServiceSource.StartActivity();
        return Convert.ToHexStringLower(GetHashBytes(input));
    }
}
