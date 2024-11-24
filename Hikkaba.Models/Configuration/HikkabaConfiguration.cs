using Hikkaba.Common.Constants;
using System.Collections.Generic;

namespace Hikkaba.Models.Configuration;

public class HikkabaConfiguration
{
    public int CacheMaxAgeSeconds { get; set; } = Defaults.DefaultAttachmentsCacheDuration;
    public int CacheCategoriesExpirationSeconds { get; set; } = Defaults.CacheCategoriesExpirationSeconds;

    public int ThumbnailsMaxWidth { get; set; } = Defaults.ThumbnailsMaxWidth;
    public int ThumbnailsMaxHeight { get; set; } = Defaults.ThumbnailsMaxHeight;

    public ICollection<string> AudioExtensions { get; set; } = Defaults.AudioExtensions;
    public ICollection<string> PictureExtensions { get; set; } = Defaults.PictureExtensions;
    public ICollection<string> VideoExtensions { get; set; } = Defaults.VideoExtensions;

    public int MaxAttachmentsCountPerPost { get; set; } = Defaults.MaxAttachmentsCountPerPost;
    public int MaxAttachmentsBytesPerPost { get; set; } = Defaults.MaxAttachmentsBytesPerPost;

    public string AuthCertificateBase64 { get; set; }
    public string AuthCertificatePassword { get; set; }
}