using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hikkaba.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime AsUtc(this DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Unspecified)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
            return dt.ToUniversalTime();
        }

        public static DateTime? AsUtc(this DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return null;
            }
            else
            {
                if (dt.Value.Kind == DateTimeKind.Unspecified)
                {
                    dt = DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
                }
                return dt.Value.ToUniversalTime();
            }
        }
    }
}
