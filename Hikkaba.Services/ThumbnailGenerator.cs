using System;
using System.IO;
using Hikkaba.Models.Dto;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Hikkaba.Services
{
    public interface IThumbnailGenerator
    {
        ThumbnailDto GenerateThumbnail(Image image, int maxWidth, int maxHeight);
    }

    public class ThumbnailGenerator : IThumbnailGenerator
    {
        public ThumbnailDto GenerateThumbnail(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var thumbnail = image.Clone(img => img.Resize(newWidth, newHeight));
            var thumbnailStream = new MemoryStream();
            thumbnail.Save(thumbnailStream, new JpegEncoder());
            thumbnailStream.Position = 0;

            return new ThumbnailDto
            {
                Image = thumbnailStream,
                Height = newHeight,
                Width = newWidth,
            };
        }
    }
}
