using Hikkaba.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Hikkaba.Tests
{
    public class IpAddressCalculatorTests
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

        private static readonly ILogger<IpAddressCalculator> Logger = Mock.Of<ILogger<IpAddressCalculator>>();

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
        public void TestIpIsInRange(string lowerInclusive, string upperInclusive, string address, bool expectedResult)
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            var actualResult = ipAddressCalculator.IsInRange(lowerInclusive, upperInclusive, address);
            if (expectedResult)
            {
                Assert.IsTrue(actualResult);
            }
            else
            {
                Assert.IsFalse(actualResult);
            }
        }
    }
}