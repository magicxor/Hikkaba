using Hikkaba.Infrastructure.Models.Attachments;
using Hikkaba.Services.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Hikkaba.Services.Implementations;

public class ThumbnailGenerator : IThumbnailGenerator
{
    public async Task<ThumbnailRm> GenerateThumbnailAsync(
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

        return new ThumbnailRm
        {
            ContentStream = thumbnailStream,
            Height = newHeight,
            Width = newWidth,
        };
    }
}
