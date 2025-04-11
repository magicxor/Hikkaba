using System.Text;
using Blake3;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Telemetry;

namespace Hikkaba.Application.Implementations;

public sealed class HashService : IHashService
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

    public byte[] GetHashBytes(byte[] input)
    {
        using var activity = ApplicationTelemetry.HashServiceSource.StartActivity();
        return Hasher.Hash(input).AsSpan().ToArray();
    }

    public byte[] GetHashBytes(Guid threadSalt, byte[] userIp)
    {
        using var activity = ApplicationTelemetry.HashServiceSource.StartActivity();

        if (threadSalt == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(threadSalt));
        }

        if (userIp == null || userIp.Length == 0 || userIp.All(b => b == 0))
        {
            throw new ArgumentNullException(nameof(userIp));
        }

        return GetHashBytes(threadSalt.ToByteArray().Concat(userIp).ToArray());
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

    public string GetHashHex(byte[] input)
    {
        using var activity = ApplicationTelemetry.HashServiceSource.StartActivity();
        return Convert.ToHexStringLower(GetHashBytes(input));
    }

    public string GetHashHex(Guid threadSalt, byte[] userIp)
    {
        using var activity = ApplicationTelemetry.HashServiceSource.StartActivity();
        return Convert.ToHexStringLower(GetHashBytes(threadSalt, userIp));
    }
}
