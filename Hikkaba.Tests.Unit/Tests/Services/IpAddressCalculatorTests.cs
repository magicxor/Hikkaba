using System.Net;
using Hikkaba.Application.Implementations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class IpAddressCalculatorTests
{
    private const string LowerLocalIpv4 = "127.0.0.1";
    private const string SomeLocalIpv4 = "127.0.0.29";
    private const string UpperLocalIpv4 = "127.0.0.255";
    private const string InvalidIpv4 = "127.0.0.256";

    private const string PrivateIpv4 = "192.168.11.26";
    private const string PublicIpv4 = "228.228.228.228";

    private const string LowerPublicIpv4 = "84.0.0.1";
    private const string SomePublicIpv4 = "84.14.88.69";
    private const string AnotherPublicIpv4 = "54.170.43.52";
    private const string UpperPublicIpv4 = "84.255.255.255";

    private const string LowerShortPublicIpv6 = "2001:4860:4860::8888";
    private const string SomeShortPublicIpv6 = "2001:4860:4860:0:f00::8888";
    private const string UpperShortPublicIpv6 = "2001:4860:ffff:ffff:ffff:ffff:ffff:0";

    private const string LowerPublicIpv6 = "2001:4860:4860:0000:0000:0000:0000:8888";
    private const string SomePublicIpv6 = "2001:4860:4860:0000:0F00:0000:0000:8888";
    private const string UpperPublicIpv6 = "2001:4860:FFFF:FFFF:FFFF:FFFF:FFFF:0000";

    private const string ShortLoopbackIpv6 = "::1";
    private const string LongLoopbackIpv6 = "0:0:0:0:0:0:0:1";
    private const string LinkLocalIpv6 = "fe80::ad88:a298:5114:18bb";

    private static readonly ILogger<IpAddressCalculator> NullLogger = NullLoggerFactory.Instance.CreateLogger<IpAddressCalculator>();

    /* Local IPv4 */
    [TestCase(LowerLocalIpv4, LowerLocalIpv4, LowerLocalIpv4, true)]
    [TestCase(UpperLocalIpv4, UpperLocalIpv4, UpperLocalIpv4, true)]
    [TestCase(LowerLocalIpv4, UpperLocalIpv4, LowerLocalIpv4, true)]
    [TestCase(LowerLocalIpv4, UpperLocalIpv4, UpperLocalIpv4, true)]
    [TestCase(LowerLocalIpv4, UpperLocalIpv4, SomeLocalIpv4, true)]
    [TestCase(LowerLocalIpv4, UpperLocalIpv4, AnotherPublicIpv4, false)]
    [TestCase(LowerLocalIpv4, SomeLocalIpv4, UpperLocalIpv4, false)]
    [TestCase(LowerLocalIpv4, UpperLocalIpv4, PrivateIpv4, false)]
    [TestCase(LowerLocalIpv4, UpperLocalIpv4, PublicIpv4, false)]
    [TestCase(LowerLocalIpv4, UpperLocalIpv4, SomePublicIpv6, false)]
    /* Public IPv4 */
    [TestCase(LowerPublicIpv4, LowerPublicIpv4, LowerPublicIpv4, true)]
    [TestCase(UpperPublicIpv4, UpperPublicIpv4, UpperPublicIpv4, true)]
    [TestCase(LowerPublicIpv4, UpperPublicIpv4, LowerPublicIpv4, true)]
    [TestCase(LowerPublicIpv4, UpperPublicIpv4, UpperPublicIpv4, true)]
    [TestCase(LowerPublicIpv4, UpperPublicIpv4, SomePublicIpv4, true)]
    [TestCase(LowerPublicIpv4, SomePublicIpv4, UpperPublicIpv4, false)]
    [TestCase(LowerPublicIpv4, UpperPublicIpv4, PrivateIpv4, false)]
    [TestCase(LowerPublicIpv4, UpperPublicIpv4, SomeLocalIpv4, false)]
    [TestCase(LowerPublicIpv4, UpperPublicIpv4, SomePublicIpv6, false)]
    /* Local IPv6 */
    [TestCase(ShortLoopbackIpv6, ShortLoopbackIpv6, ShortLoopbackIpv6, true)]
    [TestCase(LongLoopbackIpv6, LongLoopbackIpv6, LongLoopbackIpv6, true)]
    [TestCase(ShortLoopbackIpv6, LongLoopbackIpv6, LongLoopbackIpv6, true)]
    [TestCase(LongLoopbackIpv6, ShortLoopbackIpv6, ShortLoopbackIpv6, true)]
    /* Public IPv6 */
    [TestCase(LowerShortPublicIpv6, LowerShortPublicIpv6, LowerShortPublicIpv6, true)]
    [TestCase(UpperShortPublicIpv6, UpperShortPublicIpv6, UpperShortPublicIpv6, true)]
    [TestCase(LowerShortPublicIpv6, UpperShortPublicIpv6, SomeShortPublicIpv6, true)]
    [TestCase(LowerShortPublicIpv6, SomeShortPublicIpv6, UpperShortPublicIpv6, false)]
    [TestCase(LowerPublicIpv6, LowerPublicIpv6, LowerPublicIpv6, true)]
    [TestCase(UpperPublicIpv6, UpperPublicIpv6, UpperPublicIpv6, true)]
    [TestCase(LowerPublicIpv6, UpperPublicIpv6, SomePublicIpv6, true)]
    [TestCase(LowerPublicIpv6, SomePublicIpv6, UpperPublicIpv6, false)]
    [TestCase(LowerShortPublicIpv6, UpperPublicIpv6, SomeShortPublicIpv6, true)]
    [TestCase(LowerPublicIpv6, UpperShortPublicIpv6, SomeShortPublicIpv6, true)]
    [TestCase(LowerShortPublicIpv6, UpperPublicIpv6, SomePublicIpv6, true)]
    [TestCase(LowerPublicIpv6, UpperShortPublicIpv6, SomePublicIpv6, true)]
    [TestCase(ShortLoopbackIpv6, ShortLoopbackIpv6, LinkLocalIpv6, false)]
    /* Invalid combinations */
    [TestCase(LowerLocalIpv4, UpperLocalIpv4, InvalidIpv4, false)]
    [TestCase(InvalidIpv4, InvalidIpv4, InvalidIpv4, false)]
    [TestCase(InvalidIpv4, InvalidIpv4, LowerLocalIpv4, false)]
    [TestCase(LowerLocalIpv4, LinkLocalIpv6, LowerLocalIpv4, false)]
    public void TestIpIsInRangeByUpperLower(string lowerInclusive, string upperInclusive, string address, bool expectedResult)
    {
        var ipAddressCalculator = new IpAddressCalculator(NullLogger);
        var actualResult = ipAddressCalculator.IsInRange(lowerInclusive, upperInclusive, address);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase("1.0.0.0/24", "1.0.0.1", true)]
    [TestCase("1.0.224.0/19", "1.0.224.1", true)]
    [TestCase("195.49.210.0/23", "195.49.210.1", true)]
    [TestCase("223.255.254.0/24", "223.255.254.1", true)]
    [TestCase("1.0.0.0/24", "4.0.0.1", false)]
    [TestCase("1.0.224.0/19", "4.0.224.1", false)]
    [TestCase("195.49.210.0/23", "1.49.210.1", false)]
    [TestCase("223.255.254.0/24", "8.255.254.1", false)]
    [TestCase("2001:200::/37", "2001:200::1", true)]
    [TestCase("2604:de00:32::/47", "2604:de00:32::1", true)]
    [TestCase("2604:dc00:1000::/44", "2604:dc00:1000::1", true)]
    [TestCase("2c0f:fc89:809f:f3c6::/64", "2c0f:fc89:809f:f3c6::1", true)]
    public void TestIpIsInRangeBySubnet(string networkAddress, string ipAddress, bool expectedResult)
    {
        var ipAddressCalculator = new IpAddressCalculator(NullLogger);
        var actualResult = ipAddressCalculator.IsInRange(networkAddress, ipAddress);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }

    [TestCase("127.0.0.1", true)]
    [TestCase("10.0.0.1", true)]
    [TestCase("10.2.0.1", true)]
    [TestCase("172.16.0.1", true)]
    [TestCase("172.16.30.6", true)]
    [TestCase("192.168.0.1", true)]
    [TestCase("192.168.10.1", true)]
    [TestCase("::1", true)]
    [TestCase("0:0:0:0:0:0:0:1", true)]
    [TestCase("fe80::ad88:a298:5114:18bb", true)]
    [TestCase("fd12:3456:789a:1::1", true)]
    [TestCase("192.169.120.30", false)]
    [TestCase(SomePublicIpv4, false)]
    [TestCase(SomePublicIpv6, false)]
    public void TestIsPrivate(string ipAddressStr, bool expectedResult)
    {
        var ipAddressCalculator = new IpAddressCalculator(NullLogger);
        var ipAddress = IPAddress.Parse(ipAddressStr);
        var actualResult = ipAddressCalculator.IsPrivate(ipAddress);
        Assert.That(actualResult, Is.EqualTo(expectedResult));
    }
}
