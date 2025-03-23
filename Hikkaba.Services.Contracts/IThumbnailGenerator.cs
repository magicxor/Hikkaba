using Hikkaba.Infrastructure.Models;
using SixLabors.ImageSharp;

namespace Hikkaba.Services.Contracts;

public interface IThumbnailGenerator
{
    ThumbnailDto GenerateThumbnail(Image image, int maxWidth, int maxHeight);
}