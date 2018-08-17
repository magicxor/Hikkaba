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
        private const string UpperPublicIpv4 = "84.255.255.255";

        private const string ShortLoopbackIpv6 = "::1";
        private const string LongLoopbackIpv6 = "0:0:0:0:0:0:0:1";
        private const string LinkLocalIpv6 = "fe80::ad88:a298:5114:18bb";

        private static readonly ILogger<IpAddressCalculator> Logger = Mock.Of<ILogger<IpAddressCalculator>>();

        [SetUp]
        public void Setup()
        {
        }

        /* Local IPv4 */

        [Test]
        public void TestLocalIpV4Lower()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsTrue(ipAddressCalculator.IsInRange(LowerLocalIpv4, UpperLocalIpv4, LowerLocalIpv4));
        }

        [Test]
        public void TestLocalIpV4Upper()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsTrue(ipAddressCalculator.IsInRange(LowerLocalIpv4, UpperLocalIpv4, UpperLocalIpv4));
        }

        [Test]
        public void TestLocalIpV4()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsTrue(ipAddressCalculator.IsInRange(LowerLocalIpv4, UpperLocalIpv4, SomeLocalIpv4));
        }

        [Test]
        public void TestIpV4F1()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsFalse(ipAddressCalculator.IsInRange(LowerLocalIpv4, UpperLocalIpv4, PrivateIpv4));
        }

        [Test]
        public void TestIpV4F2()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsFalse(ipAddressCalculator.IsInRange(LowerLocalIpv4, UpperLocalIpv4, PublicIpv4));
        }

        /* Public IPv4 */

        [Test]
        public void TestPublicIpV4Lower()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsTrue(ipAddressCalculator.IsInRange(LowerPublicIpv4, UpperPublicIpv4, LowerPublicIpv4));
        }

        [Test]
        public void TestPublicIpV4Upper()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsTrue(ipAddressCalculator.IsInRange(LowerPublicIpv4, UpperPublicIpv4, UpperPublicIpv4));
        }

        [Test]
        public void TestPublicIpV4()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsTrue(ipAddressCalculator.IsInRange(LowerPublicIpv4, UpperPublicIpv4, SomePublicIpv4));
        }

        /* IPv6 */

        [Test]
        public void Test1LoopbackIpv6()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsTrue(ipAddressCalculator.IsInRange(ShortLoopbackIpv6, LongLoopbackIpv6, LongLoopbackIpv6));
        }

        [Test]
        public void Test2LoopbackIpv6()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsTrue(ipAddressCalculator.IsInRange(LongLoopbackIpv6, ShortLoopbackIpv6, ShortLoopbackIpv6));
        }
        
        /* Invalid combinations */

        [Test]
        public void TestInvalidIpV4()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsFalse(ipAddressCalculator.IsInRange(LowerLocalIpv4, UpperLocalIpv4, InvalidIpv4));
        }
        [Test]
        public void TestDifferentAddressFamilies()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsFalse(ipAddressCalculator.IsInRange(LowerLocalIpv4, LinkLocalIpv6, LowerLocalIpv4));
        }

        [Test]
        public void TestLinkLocalAndLocalIpV6()
        {
            var ipAddressCalculator = new IpAddressCalculator(Logger);
            Assert.IsFalse(ipAddressCalculator.IsInRange(ShortLoopbackIpv6, ShortLoopbackIpv6, LinkLocalIpv6));
        }
    }
}