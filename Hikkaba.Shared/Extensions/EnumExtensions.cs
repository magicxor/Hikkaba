using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

namespace Hikkaba.Shared.Extensions;

public static class EnumExtensions
{
    public static int ToInt<T>(this T enumValue) where T : Enum
    {
        return Convert.ToInt32(enumValue, CultureInfo.InvariantCulture);
    }

    public static EventId ToEventId<T>(this T enumValue) where T : struct, Enum
    {
        return new EventId(enumValue.ToInt(), Enum.GetName(enumValue));
    }
}
