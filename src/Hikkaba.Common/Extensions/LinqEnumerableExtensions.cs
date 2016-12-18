using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Extensions
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
