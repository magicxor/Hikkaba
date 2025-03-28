using System.ComponentModel.DataAnnotations;
using Hikkaba.Common.Constants;

namespace Hikkaba.Infrastructure.Models.Configuration;

public class HikkabaConfiguration
{
    [Required]
    [Range(5, int.MaxValue)]
    public int CacheMaxAgeSeconds { get; set; } = Defaults.DefaultAttachmentsCacheDuration;

    [Required]
    [Range(5, int.MaxValue)]
    public int CacheCategoriesExpirationSeconds { get; set; } = Defaults.CacheCategoriesExpirationSeconds;

    [Required]
    [Range(20, 4096)]
    public int ThumbnailsMaxWidth { get; set; } = Defaults.ThumbnailsMaxWidth;

    [Required]
    [Range(20, 4096)]
    public int ThumbnailsMaxHeight { get; set; } = Defaults.ThumbnailsMaxHeight;

    [Required]
    [MinLength(1)]
    public IReadOnlyCollection<string> AudioExtensions { get; set; } = Defaults.AudioExtensions;

    [Required]
    [MinLength(1)]
    public IReadOnlyCollection<string> PictureExtensions { get; set; } = Defaults.PictureExtensions;

    [Required]
    [MinLength(1)]
    public IReadOnlyCollection<string> VideoExtensions { get; set; } = Defaults.VideoExtensions;

    [Required]
    [Range(1, 100)]
    public int MaxAttachmentsCountPerPost { get; set; } = Defaults.MaxAttachmentsCountPerPost;

    [Required]
    [Range(10000, int.MaxValue)]
    public int MaxAttachmentsBytesPerPost { get; set; } = Defaults.MaxAttachmentsBytesPerPost;

    [Required]
    [MinLength(32)]
    [Base64String]
    public required string AuthCertificateBase64 { get; set; }

    [Required]
    [MinLength(8)]
    public required string AuthCertificatePassword { get; set; }

    [Required]
    [Range(1, 10)]
    public int MaxPostsFromIpWithin5Minutes { get; set; } = Defaults.MaxPostsFromIpWithin5Minutes;

    [MinLength(1)]
    public string KeysetDirectoryPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "hikkaba", "keys");
}
