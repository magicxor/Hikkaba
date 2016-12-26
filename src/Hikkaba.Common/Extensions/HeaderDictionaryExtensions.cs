using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Hikkaba.Common.Extensions
{
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
}
