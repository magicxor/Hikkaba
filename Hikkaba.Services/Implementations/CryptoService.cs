using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Hikkaba.Services.Contracts;

namespace Hikkaba.Services.Implementations;

public class CryptoService: ICryptoService
{
    private readonly HashAlgorithm _algorithm = SHA256.Create();

    private string HashEncode(byte[] hash)
    {
        return Convert.ToHexStringLower(hash);
    }

    private byte[] HashHmac(byte[] key, byte[] message)
    {
        var hash = new HMACSHA256(key);
        return hash.ComputeHash(message);
    }

    public string HashHmacHex(string key, string message)
    {
        byte[] hash = HashHmac(Encoding.Unicode.GetBytes(key), Encoding.Unicode.GetBytes(message));
        return HashEncode(hash);
    }

    private byte[] Hash(Stream inputStream)
    {
        return _algorithm.ComputeHash(inputStream);
    }

    public string HashHex(Stream inputStream)
    {
        return HashEncode(Hash(inputStream));
    }
}
