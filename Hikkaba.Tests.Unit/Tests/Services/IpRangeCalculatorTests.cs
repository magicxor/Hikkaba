using System.Net;
using Hikkaba.Application.Implementations;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public sealed class IpRangeCalculatorTests
{
    [Test]
    public void IPv4_StandardRange_ShouldReturnCorrectHosts()
    {
        var ip = IPAddress.Parse("192.168.1.0");
        IpRangeCalculator.GetHostRange(ip, 24, out var first, out var last);
        Assert.That(first, Is.EqualTo(IPAddress.Parse("192.168.1.1")));
        Assert.That(last, Is.EqualTo(IPAddress.Parse("192.168.1.254")));
    }

    [Test]
    public void IPv4_FullRange_ShouldReturnMinAndMaxIPv4()
    {
        var ip = IPAddress.Parse("0.0.0.0");
        IpRangeCalculator.GetHostRange(ip, 0, out var first, out var last);
        Assert.That(first, Is.EqualTo(IPAddress.Parse("0.0.0.1")));
        Assert.That(last, Is.EqualTo(IPAddress.Parse("255.255.255.254")));
    }

    [Test]
    public void IPv4_SingleHost_ShouldReturnSameAddress()
    {
        var ip = IPAddress.Parse("10.0.0.1");
        IpRangeCalculator.GetHostRange(ip, 32, out var first, out var last);
        Assert.That(first, Is.EqualTo(IPAddress.Parse("10.0.0.1")));
        Assert.That(last, Is.EqualTo(IPAddress.Parse("10.0.0.1")));
    }

    [Test]
    public void IPv6_StandardRange_ShouldReturnCorrectHosts()
    {
        var ip = IPAddress.Parse("2001:db8::");
        IpRangeCalculator.GetHostRange(ip, 64, out var first, out var last);
        Assert.That(first, Is.EqualTo(IPAddress.Parse("2001:db8::1")));
        Assert.That(last, Is.EqualTo(IPAddress.Parse("2001:db8::ffff:ffff:ffff:fffe")));
    }

    [Test]
    public void IPv6_SingleAddress_ShouldReturnSameAddress()
    {
        var ip = IPAddress.Parse("2001:db8::1");
        IpRangeCalculator.GetHostRange(ip, 128, out var first, out var last);
        Assert.That(first, Is.EqualTo(IPAddress.Parse("2001:db8::1")));
        Assert.That(last, Is.EqualTo(IPAddress.Parse("2001:db8::1")));
    }

    [Test]
    public void InvalidPrefixLength_TooLargeForIPv4_ShouldThrow()
    {
        var ip = IPAddress.Parse("192.168.0.0");
        Assert.That(() => IpRangeCalculator.GetHostRange(ip, 33, out _, out _), Throws.ArgumentException);
    }

    [Test]
    public void InvalidPrefixLength_Negative_ShouldThrow()
    {
        var ip = IPAddress.Parse("192.168.0.0");
        Assert.That(() => IpRangeCalculator.GetHostRange(ip, -1, out _, out _), Throws.ArgumentException);
    }

    [Test]
    public void IPv6_FullRange_ShouldReturnValidRange()
    {
        var ip = IPAddress.Parse("::");
        IpRangeCalculator.GetHostRange(ip, 0, out var first, out var last);
        Assert.That(first, Is.EqualTo(IPAddress.Parse("::1")));
        Assert.That(last, Is.EqualTo(IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:fffe")));
    }

    /* IPv4 */
    [TestCase("1.0.0.0", 24, "1.0.0.1", "1.0.0.254")]
    [TestCase("1.0.224.0", 19, "1.0.224.1", "1.0.255.254")]
    [TestCase("195.49.210.0", 23, "195.49.210.1", "195.49.211.254")]
    [TestCase("223.255.254.0", 24, "223.255.254.1", "223.255.254.254")]
    /* IPv6 */
    [TestCase("2001:200::", 37, "2001:200::1", "2001:200:7ff:ffff:ffff:ffff:ffff:fffe")]
    [TestCase("2604:de00:32::", 47, "2604:de00:32::1", "2604:de00:33:ffff:ffff:ffff:ffff:fffe")]
    [TestCase("2604:dc00:1000::", 44, "2604:dc00:1000::1", "2604:dc00:100f:ffff:ffff:ffff:ffff:fffe")]
    [TestCase("2c0f:fc89:809f:f3c6::", 64, "2c0f:fc89:809f:f3c6::1", "2c0f:fc89:809f:f3c6:ffff:ffff:ffff:fffe")]
    public void SubnetRanges_ShouldReturnExpected(string network, int prefix, string expectedFirst, string expectedLast)
    {
        var ip = IPAddress.Parse(network);
        IpRangeCalculator.GetHostRange(ip, prefix, out var first, out var last);
        Assert.That(first, Is.EqualTo(IPAddress.Parse(expectedFirst)));
        Assert.That(last, Is.EqualTo(IPAddress.Parse(expectedLast)));
    }
}
