using System;
using System.Collections.Generic;

namespace Hikkaba.Common.Constants;

public static class Defaults
{
    public const string AttachmentsStorageDirectoryName = "AttachmentsStorage";
    public const string ThumbnailPostfix = "_thumbnails";

    public const string BoardName = "Hikkaba";

    public const string AnonymousUserName = "Anonymous";

    public const string AdministratorUserName = "Administrator";
    public const string AdministratorRoleName = "administrator";

    public const int MaxIpAddressStringLength = 50;
    public const int MaxIpAddressBytesLength = 16;
    public const int MaxUserAgentLength = 500;
    public const int MaxAutonomousSystemOrganizationLength = 255;
    public const int MaxCountryIsoCodeLength = 2;

    public const int MaxAttachmentSize = 20000000;
    public const int MaxAttachmentsTotalSize = 20000000;
    public const int MaxAttachmentsCount = 6;
    public const int MaxFileNameLength = 100;
    public const int MaxFileExtensionLength = 10;
    public const int MaxFileHashBytesLength = 32;
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

    public const string DefaultMimeType = "application/octet-stream";
    public static readonly string DefaultLastModified = new DateTime(2007, 01, 01, 0, 0, 0, DateTimeKind.Utc).ToString("r"); // RFC1123
    public const int DefaultAttachmentsCacheDuration = 31536000; // ~ 1 year
    public const int CacheCategoriesExpirationSeconds = 3600;
    public const int ThumbnailsMaxWidth = 200;
    public const int ThumbnailsMaxHeight = 200;
    public const int UserIdleTimeoutMinutes = 60;
    public static IReadOnlyCollection<string> AudioExtensions { get; set; } = new List<string> { "mp3", "ogg", "aac", "m4a", "opus" }.AsReadOnly();
    public static IReadOnlyCollection<string> PictureExtensions { get; set; } = new List<string> { "jpg", "jpeg", "png", "gif", "svg", "webp", "avif" }.AsReadOnly();
    public static IReadOnlyCollection<string> VideoExtensions { get; set; } = new List<string> { "webm", "mp4" }.AsReadOnly();
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
