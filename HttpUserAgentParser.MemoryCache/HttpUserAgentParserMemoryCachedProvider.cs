// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Caching.Memory;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.MemoryCache;

/// <inheritdoc cref="IHttpUserAgentParserProvider" />
/// <summary>
/// Creates a new instance of <see cref="HttpUserAgentParserMemoryCachedProvider"/>.
/// </summary>
/// <param name="options">The options used to set expiration and size limit</param>
public sealed class HttpUserAgentParserMemoryCachedProvider(
    HttpUserAgentParserMemoryCachedProviderOptions options) : IHttpUserAgentParserProvider, IDisposable
{
    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache = new(options.CacheOptions);
    private readonly HttpUserAgentParserMemoryCachedProviderOptions _options = options;

    /// <inheritdoc/>
    public HttpUserAgentInformation Parse(string userAgent)
    {
        CacheKey key = GetKey(userAgent);

        return _memoryCache.GetOrCreate(key, static entry =>
        {
            CacheKey key = (entry.Key as CacheKey)!;
            entry.SlidingExpiration = key.Options.CacheEntryOptions.SlidingExpiration;
            entry.SetSize(1);

            return HttpUserAgentParser.Parse(key.UserAgent);
        });
    }

    [ThreadStatic]
    private static CacheKey? s_tKey;

    private CacheKey GetKey(string userAgent)
    {
        CacheKey key = s_tKey ??= new CacheKey()
        {
            UserAgent = userAgent,
            Options = _options,
        };

        return key;
    }

    private sealed class CacheKey : IEquatable<CacheKey> // required for IMemoryCache
    {
        public string UserAgent { get; init; } = null!;

        public HttpUserAgentParserMemoryCachedProviderOptions Options { get; init; } = null!;

        public bool Equals(CacheKey? other) => string.Equals(UserAgent, other?.UserAgent, StringComparison.OrdinalIgnoreCase);
        public override bool Equals(object? obj) => Equals(obj as CacheKey);

        public override int GetHashCode() => HashCode.Combine(UserAgent, Options);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }
}
