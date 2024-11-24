using System.Globalization;
using System.Resources;

namespace CodeKicker.BBCode;

internal static class MessagesHelper
{
    private static readonly ResourceManager ResMgr;

    static MessagesHelper()
    {
        ResMgr = new ResourceManager(typeof(Messages));
    }

    public static string GetString(string key)
    {
        return ResMgr.GetString(key, CultureInfo.InvariantCulture);
    }
    public static string GetString(string key, params string[] parameters)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            ResMgr.GetString(key, CultureInfo.InvariantCulture) ?? string.Empty,
            parameters);
    }
}

/// <summary>
/// reflection-only use
/// </summary>
internal static class Messages
{
}
