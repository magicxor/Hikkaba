using System;
using System.Linq;
using Hikkaba.Infrastructure.Models.Attachments;
using Hikkaba.Infrastructure.Models.Configuration;
using Hikkaba.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Hikkaba.Services.Implementations;

public class AttachmentCategorizer : IAttachmentCategorizer
{
    private readonly HikkabaConfiguration _hikkabaConfiguration;
    private readonly string[] _supportedPictureExtensions = ["jpg", "jpeg", "png", "gif", "webp", "avif"];

    public AttachmentCategorizer(IOptions<HikkabaConfiguration> settings)
    {
        _hikkabaConfiguration = settings.Value;
    }

    public bool IsPictureExtensionSupported(string extension)
    {
        return _supportedPictureExtensions.Any(ext => ext.Equals(extension, System.StringComparison.OrdinalIgnoreCase));
    }

    public Type GetAttachmentType(string fileName)
    {
        if (_hikkabaConfiguration.AudioExtensions.Any(fileName.EndsWith))
        {
            return typeof(AudioDto);
        }
        else if (_hikkabaConfiguration.PictureExtensions.Any(fileName.EndsWith))
        {
            return typeof(PictureDto);
        }
        else if (_hikkabaConfiguration.VideoExtensions.Any(fileName.EndsWith))
        {
            return typeof(VideoDto);
        }
        else
        {
            return typeof(DocumentDto);
        }
    }
}
