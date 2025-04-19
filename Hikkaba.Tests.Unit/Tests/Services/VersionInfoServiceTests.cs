using Hikkaba.Application.Implementations;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class VersionInfoServiceTests
{
    [Test]
    public void VersionInfoService_ShouldReturnVersionInfo()
    {
        var versionInfoService = new VersionInfoService();
        var versionInfo = versionInfoService.ProductVersion;
        Assert.That(versionInfo, Is.Not.Null);
        Assert.That(versionInfo, Contains.Substring("."));
    }
}
