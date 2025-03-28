using Hikkaba.Infrastructure.Models.Attachments;
using SixLabors.ImageSharp;

namespace Hikkaba.Services.Contracts;

public interface IThumbnailGenerator
{
    Task<ThumbnailRm> GenerateThumbnailAsync(Image image, int maxWidth, int maxHeight, CancellationToken cancellationToken);
}
