using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Hikkaba.Services;

public interface ICryptoService
{
    string HashHmacHex(string key, string message);
    string HashHex(Stream inputStream);
}

public class CryptoService: ICryptoService
{
    private readonly HashAlgorithm _algorithm = SHA256.Create();

    private string HashEncode(byte[] hash)
    {
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
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