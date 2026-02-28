using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace Hikkaba.Tests.Integration.Utils;

public static class StableDataGen
{
    [PublicAPI]
    [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "This is a test utility, not production code.")]
    public static Guid GenerateDeterministicGuid(int seed)
    {
        var inputBytes = BitConverter.GetBytes(seed);
        var hashBytes = MD5.HashData(inputBytes);
        return new Guid(hashBytes);
    }

    [PublicAPI]
    [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "This is a test utility, not production code.")]
    public static Guid GenerateDeterministicGuid(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        return new Guid(hashBytes);
    }

    /// <summary>
    /// Generates a deterministic 11-digit phone number from a string seed.
    /// </summary>
    [PublicAPI]
    public static string GenerateDeterministicPhone(string seed, string? salt = null)
    {
        var hash = GetStableHash(seed, salt);
        return GenerateDeterministicPhone(hash);
    }

    /// <summary>
    /// Generates a deterministic 11-digit phone number from a numeric seed.
    /// First digit is always 1–9.
    /// </summary>
    [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "This is a test utility, not production code.")]
    private static string GenerateDeterministicPhone(int seed)
    {
        var rng = new Random(seed);
        Span<char> digits = stackalloc char[11];
        digits[0] = (char)('1' + rng.Next(9));   // 1..9
        for (var i = 1; i < digits.Length; i++)
            digits[i] = (char)('0' + rng.Next(10)); // 0..9
        return new string(digits);
    }

    // FNV-1a hash algorithm for stable hashing of strings
    private static int GetStableHash(string value, string? salt = null)
    {
        if (salt != null)
            value = salt + value;

        unchecked
        {
            uint hash = 2166136261;
            foreach (var c in value)
            {
                hash ^= c;
                hash *= 16777619;
            }
            return (int)hash;
        }
    }
}
