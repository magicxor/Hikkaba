using System;

namespace Hikkaba.Tests.Unit;

internal static class TestLogUtils
{
    private static readonly TimeProvider TimeProvider = TimeProvider.System;

    public static void WriteProgressMessage(string message)
    {
        var now = TimeProvider.GetLocalNow();
        TestContext.Progress.WriteLine($"{now:HH:mm:ss.fff} {message}");
    }

    public static void WriteConsoleMessage(string message)
    {
        var now = TimeProvider.GetLocalNow();
        Console.WriteLine($@"{now:HH:mm:ss.fff} {message}");
    }
}
