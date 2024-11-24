using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Hikkaba.Infrastructure.Extensions;

public static class HeaderDictionaryExtensions
{
    public static void AddOrReplaceHeaderKey(this IHeaderDictionary headerDictionary, string key, string value)
    {
        if (headerDictionary.ContainsKey(key))
        {
            headerDictionary.Remove(key);
        }
        headerDictionary.Add(new KeyValuePair<string, StringValues>(key, value));
    }
}