using System;

namespace Hikkaba.Common.Constants
{
    public static class Defaults
    {
        public const string AttachmentsStorageDirectoryName = "AttachmentsStorage";
        public const string ThumbnailPostfix = "_thumbnails";

        public const string BoardName = "Hikkaba";

        public const string AnonymousUserName = "Anonymous";

        public const string AdministratorUserName = "Administrator";
        public const string AdministratorRoleName = "administrator";

        public const int MaxAttachmentSize = 20000000;
        public const int MaxAttachmentsTotalSize = 20000000;
        public const int MaxAttachmentsCount = 6;

        public const int MinMessageLength = 3;
        public const int MaxMessageLength = 4000;
        public const int MinTitleLength = 3;
        public const int MaxTitleLength = 100;

        public const string DefaultMimeType = "application/octet-stream";
        public static string DefaultLastModified = new DateTime(2007, 01, 01, 0, 0, 0, DateTimeKind.Utc).ToString("r"); // RFC1123
        public const int DefaultAttachmentsCacheDuration = 31556926; // ~ 1 year

        /// <summary>
        /// <para><a href="https://www.w3.org/TR/2012/WD-html-markup-20121011/datatypes.html#common.data.datetime-def">Date and time</a> (RFC 3339, ISO 8601).</para>
        /// <para>This format string should be used to output DateTime to following elements:</para>
        /// <para>- <a href="https://www.w3.org/TR/2012/WD-html-markup-20120329/input.datetime.html#input.datetime.attrs.value">input type=datetime</a></para>
        /// <para>- <a href="https://www.w3.org/TR/2012/WD-html-markup-20121011/time.html#time.attrs.datetime">time</a></para>
        /// <para>See also: <a href="https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Roundtrip">The Round-trip ("O", "o") Format Specifier</a></para>
        /// </summary>
        public const string CsharpGlobalDateTimeFormatString = "{0:o}";

        /// <summary>
        /// <para><a href="http://www.w3.org/TR/2012/WD-html-markup-20120329/datatypes.html#form.data.datetime-local-def">Local date and time</a>.</para>
        /// <para>This format string should be used to output DateTime to following elements:</para>
        /// <para>- <a href="https://www.w3.org/TR/2012/WD-html-markup-20120329/input.datetime-local.html#input.datetime-local.attrs.value">input type=datetime-local</a></para>
        /// <para>See also: <a href="https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Sortable">The Sortable ("s") Format Specifier</a></para>
        /// </summary>
        public const string CsharpLocalDateTimeFormatString = "{0:s}";

        public const string HtmlNewPostFormId = "NewPostForm";
    }
}
