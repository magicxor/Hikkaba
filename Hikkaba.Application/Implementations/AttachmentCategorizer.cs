using Hikkaba.Application.Contracts;
using Hikkaba.Shared.Constants;
using Hikkaba.Shared.Enums;

namespace Hikkaba.Application.Implementations;

public sealed class AttachmentCategorizer : IAttachmentCategorizer
{
    public bool IsPictureExtensionSupported(string extension)
    {
        return Defaults.SupportedPictureExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
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
