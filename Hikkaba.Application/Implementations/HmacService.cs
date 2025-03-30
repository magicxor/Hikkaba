using System.Security.Cryptography;
using System.Text;
using Hikkaba.Application.Contracts;

namespace Hikkaba.Application.Implementations;

public class HmacService : IHmacService
{
    private static byte[] HashHmac(byte[] key, byte[] message)
    {
        var hash = new HMACSHA3_512(key);
        return hash.ComputeHash(message);
    }

    public string HashHmacHex(string key, string message)
    {
        var hash = HashHmac(Encoding.Unicode.GetBytes(key), Encoding.Unicode.GetBytes(message));
        return Convert.ToHexStringLower(hash);
    }
}
