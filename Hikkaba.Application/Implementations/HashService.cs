using System.Text;
using Blake3;
using Hikkaba.Application.Contracts;

namespace Hikkaba.Application.Implementations;

public class HashService : IHashService
{
    public byte[] GetHashBytes(Stream inputStream)
    {
        using var blake3Stream = new Blake3Stream(inputStream);
        return blake3Stream.ComputeHash().AsSpan().ToArray();
    }

    public byte[] GetHashBytes(string input)
    {
        return Hasher.Hash(Encoding.UTF8.GetBytes(input)).AsSpan().ToArray();
    }

    public string GetHashHex(Stream inputStream)
    {
        return Convert.ToHexStringLower(GetHashBytes(inputStream));
    }

    public string GetHashHex(string input)
    {
        return Convert.ToHexStringLower(GetHashBytes(input));
    }
}
