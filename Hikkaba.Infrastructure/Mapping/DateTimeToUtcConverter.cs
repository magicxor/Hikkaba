using System;
using AutoMapper;
using Hikkaba.Infrastructure.Extensions;

namespace Hikkaba.Infrastructure.Mapping;

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