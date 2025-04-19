using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Application.Implementations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Hikkaba.Tests.Unit.Tests.Services;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal sealed class ThumbnailGeneratorTests
{
    private static readonly DecoderOptions DecoderOptions = new() { MaxFrames = 1 };

    [CancelAfter(TestDefaults.TestTimeout)]
    [TestCase("gif_animated.gif")]
    [TestCase("jpg_xyb.jpg")]
    [TestCase("jpg_very_small.jpg")]
    [TestCase("jpg.jpg")]
    [TestCase("jpg_corrupted_1.jpg")]
    [TestCase("jpg_corrupted_2.jpg")]
    [TestCase("png_animated.png")]
    [TestCase("png_static.png")]
    [TestCase("png_very_large.png")]
    [TestCase("webp_animated.webp")]
    [TestCase("webp_static.webp")]
    public async Task GenerateThumbnailAsync_ShouldReturnCorrectThumbnail(string fileName, CancellationToken cancellationToken)
    {
        var thumbnailGenerator = new ThumbnailGenerator();
        await using var fileStream = new FileStream(Path.Combine("Files", fileName), FileMode.Open);
        using var image = await Image.LoadAsync(DecoderOptions, fileStream, cancellationToken);
        await using var thumbnail = await thumbnailGenerator.GenerateThumbnailAsync(image, 100, 100, cancellationToken);

        Assert.That(thumbnail, Is.Not.Null);
        Assert.That(thumbnail.ContentStream, Is.Not.Null);
        Assert.That(thumbnail.ContentStream.Length, Is.GreaterThan(0));
        Assert.That(thumbnail.Extension, Is.Not.Null);
        Assert.That(thumbnail.Extension, Is.Not.Empty);
        Assert.That(thumbnail.Width, Is.GreaterThan(0));
        Assert.That(thumbnail.Height, Is.GreaterThan(0));
        Assert.That(thumbnail.Width, Is.LessThanOrEqualTo(100));
        Assert.That(thumbnail.Height, Is.LessThanOrEqualTo(100));
        Assert.That(thumbnail.ContentStream.CanRead, Is.True);
    }
}
