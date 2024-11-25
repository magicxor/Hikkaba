using Hikkaba.Models.Dto;
using SixLabors.ImageSharp;

namespace Hikkaba.Services.Contracts;

public interface IThumbnailGenerator
{
    ThumbnailDto GenerateThumbnail(Image image, int maxWidth, int maxHeight);
}