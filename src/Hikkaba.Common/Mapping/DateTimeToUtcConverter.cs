using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hikkaba.Common.Extensions;

namespace Hikkaba.Common.Mapping
{
    public class DateTimeToUtcConverter : ITypeConverter<DateTime, DateTime>
    {
        public DateTime Convert(DateTime source, DateTime destination, ResolutionContext context)
        {
            return source.AsUtc();
        }
    }

    public class NullableDateTimeToUtcConverter : ITypeConverter<DateTime?, DateTime?>
    {
        public DateTime? Convert(DateTime? source, DateTime? destination, ResolutionContext context)
        {
            return source.AsUtc();
        }
    }
}
