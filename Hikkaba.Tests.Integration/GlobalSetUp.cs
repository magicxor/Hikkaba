using Hikkaba.Tests.Integration.Extensions;
using Hikkaba.Tests.Integration.Services;
using Hikkaba.Tests.Integration.Utils;
using NLog;

namespace Hikkaba.Tests.Integration;

[SetUpFixture]
public sealed class GlobalSetUp
{
    private static DbContainerManager? GlobalDbContainerManager { get; set; }

    private static string? GlobalDbHost { get; set; }
    private static ushort? GlobalDbPort { get; set; }

    public static string DbHost => GlobalDbHost ?? throw new InvalidOperationException("DbHost not initialized");
    public static ushort DbPort => GlobalDbPort ?? throw new InvalidOperationException("DbPort not initialized");

    [OneTimeSetUp]
    public async Task RunBeforeAnyTestsAsync()
    {
        GlobalDbContainerManager = new DbContainerManager();

        var containerInfo = await GlobalDbContainerManager.StartAsync();

        GlobalDbHost = containerInfo.Host;
        GlobalDbPort = containerInfo.Port;
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTestsAsync()
    {
        TestDbUtils.DisposePools();
        await GlobalDbContainerManager.StopIfNotNullAsync();
        LogManager.Shutdown();
    }
}
