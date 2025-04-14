namespace Hikkaba.Shared.Enums;

public enum PostingRestrictionType
{
    /// <summary>
    /// User can post without any restrictions.
    /// </summary>
    NoRestriction = 0,

    /// <summary>
    /// User must wait for a certain period of time before posting again.
    /// </summary>
    RateLimitExceeded = 1,

    /// <summary>
    /// User is banned from posting due to a violation of the rules.
    /// </summary>
    IpAddressBanned = 2,

    /// <summary>
    /// Country is banned from posting.
    /// </summary>
    CountryBanned = 3,

    /// <summary>
    /// User is using a proxy/VPN/TOR/etc. and posting is not allowed.
    /// </summary>
    ProxyNotAllowed = 4,

    /// <summary>
    /// Thread is closed and nobody can post in it.
    /// </summary>
    ThreadClosed = 5,

    /// <summary>
    /// Thread is deleted or does not exist.
    /// </summary>
    ThreadNotFound = 6,

    /// <summary>
    /// Category is deleted or does not exist.
    /// </summary>
    CategoryNotFound = 7,

    /// <summary>
    /// User's IP address cannot be determined.
    /// </summary>
    IpAddressNotFound = 8,
}
