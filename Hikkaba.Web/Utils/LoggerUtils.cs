using Hikkaba.Shared.Utils;
using NLog;

namespace Hikkaba.Web.Utils;

public static class LoggerUtils
{
    public static bool IsIntegrationTestLogging()
    {
        return LogManager.Configuration?.FindTargetByName("integrationTest") != null
               || EnvironmentHelper.GetEnvironmentName() == EnvironmentHelper.IntegrationTestsEnvironmentName;
    }
}
