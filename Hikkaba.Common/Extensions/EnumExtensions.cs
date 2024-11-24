using Microsoft.Extensions.Logging;
using System;

namespace Hikkaba.Common.Extensions;

public static class EnumExtensions
{
    public static int ToInt<T>(this T enumValue) where T : Enum
    {
        return Convert.ToInt32(enumValue);
    }

    public static EventId ToEventId<T>(this T enumValue) where T : Enum
    {
        return new EventId(enumValue.ToInt(), enumValue.ToString());
    }
}
