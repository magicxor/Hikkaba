using System;
using System.IO;
using System.Linq;
using System.Text;
using Hikkaba.Application.Implementations;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class HashServiceTests
{
    [TestCase("hello world", "d74981efa70a0c880b8d8c1985d075dbcbf679b99a5f9914e5aaf96b831a9e24")]
    [TestCase("привет, мир", "ba03fa593c2f4415508b7df367e2db2190c8f7dcf2e9a822a7938ecc60d8f0af")]
    public void GetHash_ShouldReturnCorrectHash(string input, string expectedHash)
    {
        var hashService = new HashService();
        var hash = hashService.GetHashHex(input);
        Assert.That(hash, Is.EqualTo(expectedHash));

        var bytes = Encoding.UTF8.GetBytes(input);
        var hashFromBytes = hashService.GetHashHex(bytes);
        Assert.That(hashFromBytes, Is.EqualTo(expectedHash));

        using var stream = new MemoryStream(bytes);
        var hashFromStream = hashService.GetHashHex(stream);
        Assert.That(hashFromStream, Is.EqualTo(expectedHash));
    }

    [TestCase("caa02d7a-2a1a-4d65-9ff5-31497715caed", "hello world", "d74981efa70a0c880b8d8c1985d075dbcbf679b99a5f9914e5aaf96b831a9e24")]
    [TestCase("94886bd8-6911-423d-af65-dcbe30d57fe9", "привет, мир", "ba03fa593c2f4415508b7df367e2db2190c8f7dcf2e9a822a7938ecc60d8f0af")]
    public void GetHashWithSalt_ShouldReturnCorrectHash(string salt, string input, string expectedHash)
    {
        var guidSalt = Guid.Parse(salt);
        var hashService = new HashService();
        var hash1 = hashService.GetHashHex(guidSalt, Encoding.UTF8.GetBytes(input));
        var hash1Bytes = hashService.GetHashBytes(guidSalt, Encoding.UTF8.GetBytes(input));

        var inputBytes = guidSalt.ToByteArray().Concat(Encoding.UTF8.GetBytes(input)).ToArray();
        var hash2 = hashService.GetHashHex([..inputBytes]);
        var hash2Bytes = hashService.GetHashBytes([..inputBytes]);

        Assert.That(hash1, Is.EqualTo(hash2));
        Assert.That(hash1Bytes, Is.EqualTo(hash2Bytes));
    }
}
