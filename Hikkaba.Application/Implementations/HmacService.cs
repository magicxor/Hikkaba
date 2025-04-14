using System.Security.Cryptography;
using System.Text;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Telemetry;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Shared.Constants;
using Microsoft.Extensions.Options;

namespace Hikkaba.Application.Implementations;

public sealed class HmacService : IHmacService
{
    private readonly IOptions<HikkabaConfiguration> _options;

    public HmacService(IOptions<HikkabaConfiguration> options)
    {
        _options = options;
    }

    public byte[] HashHmac(byte[] key, byte[] message)
    {
        using var activity = ApplicationTelemetry.HashServiceSource.StartActivity();
        using var hash = new HMACSHA3_256(key);
        return hash.ComputeHash(message);
    }

    public byte[] HashHmac(string key, string message)
    {
        return HashHmac(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(message));
    }

    public string GetTripCode(string input)
    {
        input = input.Trim();

        var values = input.Split("##", 2, StringSplitOptions.TrimEntries);
        if (values.Length != 2)
        {
            throw new ArgumentException("Input must contain '##' separator", nameof(input));
        }

        var name = values[0];
        var password = values[1];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password part cannot be empty", nameof(input));
        }

        if (password.Length < Defaults.MinTripCodePasswordLength)
        {
            throw new ArgumentException($"Password must be at least {Defaults.MinTripCodePasswordLength} characters", nameof(input));
        }

        if (password.Length > Defaults.MaxTripCodePasswordLength)
        {
            throw new ArgumentException($"Password part cannot exceed {Defaults.MaxTripCodePasswordLength} characters", nameof(input));
        }

        if (name.Length > Defaults.MaxNameLength)
        {
            throw new ArgumentException($"Name part cannot exceed {Defaults.MaxNameLength} characters", nameof(input));
        }

        var tripCodeSalt = _options.Value.TripCodeSalt;
        var tripCodeHash = HashHmac(tripCodeSalt, input);
        var tripCodeHashShort = tripCodeHash.AsSpan(0, 9);
        var tripCodeHashBase64 = Convert.ToBase64String(tripCodeHashShort);

        return $"{name}!!{tripCodeHashBase64}";
    }
}
