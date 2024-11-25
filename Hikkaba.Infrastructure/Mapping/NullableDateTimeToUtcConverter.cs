using System;
using AutoMapper;
using Hikkaba.Infrastructure.Extensions;

namespace Hikkaba.Infrastructure.Mapping;

public class NullableDateTimeToUtcConverter : ITypeConverter<DateTime?, DateTime?>
{
    public DateTime? Convert(DateTime? source, DateTime? destination, ResolutionContext context)
    {
        return source.AsUtc();
    }
}