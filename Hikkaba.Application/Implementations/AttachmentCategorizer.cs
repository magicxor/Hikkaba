using Hikkaba.Application.Contracts;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;
using Microsoft.Extensions.Options;

namespace Hikkaba.Application.Implementations;

public class AttachmentCategorizer : IAttachmentCategorizer
{
    private readonly HikkabaConfiguration _hikkabaConfiguration;

    public AttachmentCategorizer(IOptions<HikkabaConfiguration> settings)
    {
        _hikkabaConfiguration = settings.Value;
    }

    public bool IsPictureExtensionSupported(string extension)
    {
        return Defaults.SupportedPictureExtensions.Any(ext => ext.Equals(extension, System.StringComparison.OrdinalIgnoreCase));
    }

    public AttachmentType GetAttachmentType(string extension)
    {
        if (Defaults.SupportedAudioExtensions.Any(x => x == extension))
        {
            return AttachmentType.Audio;
        }
        else if (Defaults.SupportedPictureExtensions.Any(x => x == extension))
        {
            return AttachmentType.Picture;
        }
        else if (Defaults.SupportedVideoExtensions.Any(x => x == extension))
        {
            return AttachmentType.Video;
        }
        else
        {
            return AttachmentType.Document;
        }
    }
}
