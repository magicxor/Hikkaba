// Copyright © https://myCSharp.de - all rights reserved

using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Information about the user agent platform
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="HttpUserAgentPlatformInformation"/>
/// </remarks>
public readonly struct HttpUserAgentPlatformInformation(Regex regex, string name, HttpUserAgentPlatformType platformType) : IEquatable<HttpUserAgentPlatformInformation>
{
    /// <summary>
    /// Regex-pattern that matches this user agent string
    /// </summary>
    public Regex Regex { get; } = regex;

    /// <summary>
    /// Name of the platform
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Specific platform type aka family
    /// </summary>
    public HttpUserAgentPlatformType PlatformType { get; } = platformType;

    public override int GetHashCode() => HashCode.Combine(Regex, Name, PlatformType);

    public override bool Equals(object? obj)
    {
        if (obj is not HttpUserAgentPlatformInformation other)
            return false;

        return Regex.Equals(other.Regex)
               && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
               && PlatformType == other.PlatformType;
    }

    public bool Equals(HttpUserAgentPlatformInformation other)
    {
        return Regex.Equals(other.Regex)
               && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
               && PlatformType == other.PlatformType;
    }

    public override string ToString()
    {
        return $"{nameof(HttpUserAgentPlatformInformation)}: {Name} ({PlatformType})";
    }

    public static bool operator ==(HttpUserAgentPlatformInformation left, HttpUserAgentPlatformInformation right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HttpUserAgentPlatformInformation left, HttpUserAgentPlatformInformation right)
    {
        return !(left == right);
    }
}
