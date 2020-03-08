using System.Linq;
using Hikkaba.Models.Configuration;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Models.Dto.Attachments.Base;
using Microsoft.Extensions.Options;

namespace Hikkaba.Services
{
    public interface IAttachmentCategorizer
    {
        bool IsPictureExtensionSupported(string extension);
        FileAttachmentDto CreateAttachmentDto(string fileName);
    }

    public class AttachmentCategorizer : IAttachmentCategorizer
    {
        private readonly HikkabaConfiguration _hikkabaConfiguration;
        private readonly string[] _supportedPictureExtensions = { "jpg", "jpeg", "png", "gif" };

        public AttachmentCategorizer(IOptions<HikkabaConfiguration> settings)
        {
            _hikkabaConfiguration = settings.Value;
        }

        public bool IsPictureExtensionSupported(string extension)
        {
            return _supportedPictureExtensions.Any(ext => ext.Equals(extension));
        }

        public FileAttachmentDto CreateAttachmentDto(string fileName)
        {
            if (_hikkabaConfiguration.AudioExtensions.Any(fileName.EndsWith))
            {
                return new AudioDto();
            }
            else if (_hikkabaConfiguration.PictureExtensions.Any(fileName.EndsWith))
            {
                return new PictureDto();
            }
            else if (_hikkabaConfiguration.VideoExtensions.Any(fileName.EndsWith))
            {
                return new VideoDto();
            }
            else
            {
                return new DocumentDto();
            }
        }
    }
}