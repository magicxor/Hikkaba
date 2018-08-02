using System.Linq;
using Hikkaba.Models.Configuration;
using Hikkaba.Models.Dto.Attachments;
using Hikkaba.Models.Dto.Attachments.Base;
using Microsoft.Extensions.Options;

namespace Hikkaba.Service
{
    public interface IAttachmentCategorizer
    {
        bool IsPictureExtensionSupported(string extension);
        FileAttachmentDto GetAttachmentTypeByFileName(string fileName);
    }

    public class AttachmentCategorizer: IAttachmentCategorizer
    {
        private readonly HikkabaConfiguration _hikkabaConfiguration;
        private readonly string[] _supportedPictureExtensions = {"jpg", "jpeg", "png", "gif"};

        public AttachmentCategorizer(IOptions<HikkabaConfiguration> settings)
        {
            _hikkabaConfiguration = settings.Value;
        }

        private T Create<T>() where T : class, new()
        {
            return new T();
        }

        public bool IsPictureExtensionSupported(string extension)
        {
            return _supportedPictureExtensions.Any(x => x.Equals(extension));
        }

        public FileAttachmentDto GetAttachmentTypeByFileName(string fileName)
        {
            if (_hikkabaConfiguration.AudioExtensions.Any(fileName.EndsWith))
            {
                return Create<AudioDto>();
            }
            else if (_hikkabaConfiguration.PictureExtensions.Any(fileName.EndsWith))
            {
                return Create<PictureDto>();
            }
            else if (_hikkabaConfiguration.VideoExtensions.Any(fileName.EndsWith))
            {
                return Create<VideoDto>();
            }
            else
            {
                return Create<DocumentDto>();
            }
        }
    }
}