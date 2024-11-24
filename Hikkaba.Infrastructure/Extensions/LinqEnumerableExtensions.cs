using System;
using System.Collections.Generic;

namespace Hikkaba.Infrastructure.Extensions;

public static class LinqEnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
    {
        foreach (var item in enumeration)
        {
            action(item);
        }
    }
        
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<int, T> action)
    {
        var i = 0;
        foreach (var item in enumeration)
        {
            action(i, item);
            i++;
        }
    }
}