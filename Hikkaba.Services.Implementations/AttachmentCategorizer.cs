using Hikkaba.Common.Enums;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Hikkaba.Services.Implementations;

public class AttachmentCategorizer : IAttachmentCategorizer
{
    private readonly HikkabaConfiguration _hikkabaConfiguration;
    private readonly string[] _supportedPictureExtensions = ["jpg", "jpeg", "png", "gif", "webp"];

    public AttachmentCategorizer(IOptions<HikkabaConfiguration> settings)
    {
        _hikkabaConfiguration = settings.Value;
    }

    public bool IsPictureExtensionSupported(string extension)
    {
        return _supportedPictureExtensions.Any(ext => ext.Equals(extension, System.StringComparison.OrdinalIgnoreCase));
    }

    public AttachmentType GetAttachmentType(string extension)
    {
        if (_hikkabaConfiguration.AudioExtensions.Any(x => x == extension))
        {
            return AttachmentType.Audio;
        }
        else if (_hikkabaConfiguration.PictureExtensions.Any(x => x == extension))
        {
            return AttachmentType.Picture;
        }
        else if (_hikkabaConfiguration.VideoExtensions.Any(x => x == extension))
        {
            return AttachmentType.Video;
        }
        else
        {
            return AttachmentType.Document;
        }
    }
}
