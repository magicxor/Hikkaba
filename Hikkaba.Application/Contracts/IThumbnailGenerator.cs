using Hikkaba.Infrastructure.Models.Attachments.StreamContainers;
using JetBrains.Annotations;
using SixLabors.ImageSharp;

namespace Hikkaba.Application.Contracts;

public interface IThumbnailGenerator
{
    [MustDisposeResource]
    Task<ThumbnailStreamContainer> GenerateThumbnailAsync(Image image, int maxWidth, int maxHeight, CancellationToken cancellationToken);
}
