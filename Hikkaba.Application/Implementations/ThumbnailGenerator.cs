using System.Diagnostics;
using Hikkaba.Application.Contracts;
using Hikkaba.Application.Telemetry;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Hikkaba.Application.Implementations;

public sealed class ThumbnailGenerator : IThumbnailGenerator
{
    private static readonly IReadOnlyList<string> FormatsWithTransparencySupport = [
        SixLabors.ImageSharp.Formats.Gif.GifFormat.Instance.Name,
        SixLabors.ImageSharp.Formats.Png.PngFormat.Instance.Name,
        SixLabors.ImageSharp.Formats.Tiff.TiffFormat.Instance.Name,
        SixLabors.ImageSharp.Formats.Tga.TgaFormat.Instance.Name,
        SixLabors.ImageSharp.Formats.Webp.WebpFormat.Instance.Name,
        SixLabors.ImageSharp.Formats.Qoi.QoiFormat.Instance.Name,
    ];

    [MustDisposeResource]
    public async Task<ThumbnailStreamContainer> GenerateThumbnailAsync(
        Image image,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken)
    {
        using var activity = ApplicationTelemetry.ThumbnailGeneratorSource.StartActivity();

        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        var thumbnail = image.Clone(img => img.Resize(newWidth, newHeight));
        activity?.AddEvent(new ActivityEvent("Thumbnail resized"));

        var thumbnailStream = new MemoryStream();
        string extension;

        if (image.Metadata.DecodedImageFormat?.Name is { } imgFormatName
            && FormatsWithTransparencySupport.Contains(imgFormatName))
        {
            extension = "png";
            await thumbnail.SaveAsPngAsync(thumbnailStream, cancellationToken);
        }
        else
        {
            extension = "jpg";
            await thumbnail.SaveAsJpegAsync(thumbnailStream, cancellationToken);
        }

        thumbnailStream.Position = 0;
        activity?.AddEvent(new ActivityEvent("Thumbnail saved to memory stream as JPEG"));

        return new ThumbnailStreamContainer
        {
            ContentStream = thumbnailStream,
            Height = newHeight,
            Extension = extension,
            Width = newWidth,
        };
    }
}
