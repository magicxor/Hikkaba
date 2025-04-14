using System;
using System.Collections.Generic;
using System.Linq;

namespace Hikkaba.Shared.Constants;

public static class Defaults
{
    public const string AttachmentsStorageDirectoryName = "AttachmentsStorage";
    public const string ThumbnailPostfix = "_thumbnail";

    public const string ServiceName = "Hikkaba";
    public const string BoardName = "Hikkaba";

    public const string AnonymousUserName = "Anonymous";

    public const string AdministratorUserName = "admin";
    public const string AdministratorRoleName = "administrator";
    public const string ModeratorRoleName = "moderator";

    public const string DeniedCategoryNameAll = "all";
    public const string DeniedCategoryNameAdmin = "admin";

    public const int MinUserPasswordLength = 8;
    public const int MaxUserPasswordLength = 100;
    public const int MaxGuidLength = 36;
    public const int MaxNameLength = 100;
    public const int MaxEmailLength = 100;
    public const int MinTripCodePasswordLength = 8;
    public const int MaxTripCodePasswordLength = 100;
    public const int MaxIpAddressStringLength = 50;
    public const int MaxIpAddressBytesLength = 16;
    public const int MaxUserAgentLength = 500;
    public const int MaxAutonomousSystemOrganizationLength = 255;
    public const int MaxCountryIsoCodeLength = 2;
    public const int MaxThreadCountInCategory = 5000;
    public const int MaxAttachmentSize = 20000000;
    public const int MaxAttachmentsTotalSize = 20000000;
    public const int MaxAttachmentsCount = 6;
    public const int MaxFileNameLength = 100;
    public const int MaxFileExtensionLength = 10;
    public const int MaxFileHashBytesLength = 32;
    public const int MaxFileContentTypeLength = 255;
    public const int MinSearchTermLength = 3;
    public const int MaxSearchTermLength = 100;
    public const int MinCategoryAliasLength = 1;
    public const int MaxCategoryAliasLength = 10;
    public const int MinCategoryAndBoardNameLength = 2;
    public const int MaxCategoryAndBoardNameLength = 100;
    public const int MaxMessageLength = 4000;
    public const int MaxMessageHtmlLength = 8192;
    public const int MinTitleLength = 2;
    public const int MaxTitleLength = 100;
    public const int MaxReasonLength = 500;
    public const int MaxNoticeLength = 500;
    public const int MinBumpLimit = 500;
    public const int MaxBumpLimit = 2000;
    public const int DefaultBumpLimit = 500;
    public const int MaxAttachmentsCountPerPost = 10;
    public const int MaxAttachmentsBytesPerPost = 20000000;
    public const int LatestPostsCountInThreadPreview = 3;
    public const int MaxPostsFromIpWithin5Minutes = 2;

    public const string DefaultMimeType = "application/octet-stream";
    public static readonly string DefaultLastModified = new DateTime(2007, 01, 01, 0, 0, 0, DateTimeKind.Utc).ToString("r"); // RFC1123
    public const int DefaultAttachmentsCacheDuration = 31536000; // ~ 1 year
    public const int CacheCategoriesExpirationSeconds = 3600;
    public static readonly int CacheMaxAgeSeconds = Convert.ToInt32(TimeSpan.FromDays(365).TotalSeconds);
    public const int ThumbnailsMaxWidth = 200;
    public const int ThumbnailsMaxHeight = 200;
    public const int UserIdleTimeoutMinutes = 60;

    public const int HikkabaStartEventId = 735060000;

    public const string AudioExtensions = "mp3,ogg,aac,m4a,opus";
    public const string PictureExtensions = "jpg,jpeg,png,gif,webp";
    public const string VideoExtensions = "webm,mp4";
    public const string AllAllowedExtensions = AudioExtensions + "," + PictureExtensions + "," + VideoExtensions;

    public static readonly IReadOnlyCollection<string> SupportedAudioExtensions = AudioExtensions.Split(',');
    public static readonly IReadOnlyCollection<string> SupportedPictureExtensions = PictureExtensions.Split(',');
    public static readonly IReadOnlyCollection<string> SupportedVideoExtensions = VideoExtensions.Split(',');

    public static readonly string AllSupportedExtensionsWithDot = string.Join(",",
        SupportedAudioExtensions
        .Concat(SupportedPictureExtensions)
        .Concat(SupportedVideoExtensions)
        .Select(x => "." + x));

    public const string AspNetEnvIntegrationTesting = "IntegrationTesting";

    /// <summary>
    /// <para><a href="https://www.w3.org/TR/2012/WD-html-markup-20121011/datatypes.html#common.data.datetime-def">Date and time</a> (RFC 3339, ISO 8601).</para>
    /// <para>This format string should be used to output DateTime to following elements:</para>
    /// <para>- <a href="https://www.w3.org/TR/2012/WD-html-markup-20120329/input.datetime.html#input.datetime.attrs.value">input type=datetime</a></para>
    /// <para>- <a href="https://www.w3.org/TR/2012/WD-html-markup-20121011/time.html#time.attrs.datetime">time</a></para>
    /// <para>See also: <a href="https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Roundtrip">The Round-trip ("O", "o") Format Specifier</a></para>
    /// </summary>
    /// <remarks>w3c: A fraction of a second must be one, two, or three digits.</remarks>
    public const string CsharpGlobalDateTimeFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK";

    /// <summary>
    /// <para><a href="http://www.w3.org/TR/2012/WD-html-markup-20120329/datatypes.html#form.data.datetime-local-def">Local date and time</a>.</para>
    /// <para>This format string should be used to output DateTime to following elements:</para>
    /// <para>- <a href="https://www.w3.org/TR/2012/WD-html-markup-20120329/input.datetime-local.html#input.datetime-local.attrs.value">input type=datetime-local</a></para>
    /// <para>See also: <a href="https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Sortable">The Sortable ("s") Format Specifier</a></para>
    /// </summary>
    public const string CsharpLocalDateTimeFormatString = "{0:s}";

    public const string HtmlNewPostFormId = "NewPostForm";

    public const string CacheKeyCategories = "categories";
}
