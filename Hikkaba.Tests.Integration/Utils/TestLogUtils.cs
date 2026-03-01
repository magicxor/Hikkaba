namespace Hikkaba.Tests.Integration.Utils;

public static class TestLogUtils
{
    private static readonly bool IsDebugLoggingEnabled = bool.Parse("false");
    private static readonly TimeProvider TimeProvider = TimeProvider.System;

    public static void WriteProgressMessage(string message)
    {
        if (!IsDebugLoggingEnabled)
        {
            return;
        }

        var now = TimeProvider.GetLocalNow();
        TestContext.Progress.WriteLine($"{now:HH:mm:ss.fff} {message}");
    }
}
