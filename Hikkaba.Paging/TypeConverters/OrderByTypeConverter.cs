using System.ComponentModel;
using System.Globalization;
using Hikkaba.Paging.Enums;
using Hikkaba.Paging.Models;
using JetBrains.Annotations;

namespace Hikkaba.Paging.TypeConverters;

/// <summary>
/// Order by type converter.
/// </summary>
[PublicAPI]
[UsedImplicitly]
public class OrderByTypeConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context,
        Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value)
    {
        switch (value)
        {
            case null:
            {
                return null;
            }

            case string stringValue:
            {
                string? field;
                var direction = OrderByDirection.Asc;

                if (stringValue.Contains(',', StringComparison.Ordinal))
                {
                    var parts = stringValue.Split(',');
                    field = parts[0];
                    direction = parts[1] == "desc" ? OrderByDirection.Desc : OrderByDirection.Asc;
                }
                else
                {
                    field = stringValue;
                }

                var orderByItem = new OrderByItem
                {
                    Field = field,
                    Direction = direction,
                };

                return orderByItem;
            }

            default:
                return base.ConvertFrom(context, culture, value);
        }
    }
}
