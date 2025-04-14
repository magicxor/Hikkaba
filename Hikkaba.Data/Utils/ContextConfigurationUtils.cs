using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hikkaba.Data.Utils;

internal static class ContextConfigurationUtils
{
    private static readonly ValueConverter DateTimeUtcValueConverter =
        new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    private static readonly ValueConverter DateTimeUtcNullableValueConverter =
        new ValueConverter<DateTime?, DateTime?>(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    internal static void SetValueConverters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(DateTimeUtcValueConverter);
                }

                if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(DateTimeUtcNullableValueConverter);
                }
            }
        }
    }
}
