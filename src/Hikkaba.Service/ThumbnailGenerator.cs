using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Dto;
using ImageSharp;

namespace Hikkaba.Service
{
    public interface IThumbnailGenerator
    {
        ThumbnailDto GenerateThumbnail(Image image, int maxWidth, int maxHeight);
    }

    public class ThumbnailGenerator: IThumbnailGenerator
    {
        public ThumbnailDto GenerateThumbnail(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var thumbnail = image.Resize(newWidth, newHeight);
            var thumbnailStream = new MemoryStream();
            thumbnail.Save(thumbnailStream);
            thumbnailStream.Position = 0;

            return new ThumbnailDto()
            {
                Image = thumbnailStream,
                Height = newHeight,
                Width = newWidth,
            };
        }
    }
}
