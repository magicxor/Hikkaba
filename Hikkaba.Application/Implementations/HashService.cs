using System.Buffers;
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

        const int bufferSize = 8192;
        using var hasher = Hasher.New();
        var sharedArrayPool = ArrayPool<byte>.Shared;
        var buffer = sharedArrayPool.Rent(bufferSize);
        Array.Fill<byte>(buffer, 0);
        try
        {
            for (int read; (read = inputStream.Read(buffer, 0, buffer.Length)) != 0;)
            {
                hasher.Update(buffer.AsSpan(start: 0, read));
            }

            return hasher.Finalize().AsSpan().ToArray();
        }
        finally
        {
            sharedArrayPool.Return(buffer);
        }
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
