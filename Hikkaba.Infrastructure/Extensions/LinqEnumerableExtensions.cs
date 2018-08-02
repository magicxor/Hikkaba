using System;
using System.Collections.Generic;

namespace Hikkaba.Infrastructure.Extensions
{
    public static class LinqEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
    }
}
