using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Hikkaba.Application.Implementations;

public class ThumbnailGenerator : IThumbnailGenerator
{
    [MustDisposeResource]
    public async Task<ThumbnailStreamContainer> GenerateThumbnailAsync(
        Image image,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken)
    {
        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        var thumbnail = image.Clone(img => img.Resize(newWidth, newHeight));
        var thumbnailStream = new MemoryStream();
        await thumbnail.SaveAsJpegAsync(thumbnailStream, cancellationToken: cancellationToken);
        thumbnailStream.Position = 0;

        return new ThumbnailStreamContainer
        {
            ContentStream = thumbnailStream,
            Height = newHeight,
            Width = newWidth,
        };
    }
}
