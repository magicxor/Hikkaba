using System.Resources;

namespace CodeKicker.BBCode;

internal static class MessagesHelper
{
    private static readonly ResourceManager resMgr;

    static MessagesHelper()
    {
        resMgr = new ResourceManager(typeof(Messages));
    }

    public static string GetString(string key)
    {
        return resMgr.GetString(key);
    }
    public static string GetString(string key, params string[] parameters)
    {
        return string.Format(resMgr.GetString(key), parameters);
    }
}

/// <summary>
/// reflection-only use
/// </summary>
internal static class Messages
{
}
