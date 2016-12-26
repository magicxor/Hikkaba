using System;

namespace Hikkaba.Common.Constants
{
    public static class Defaults
    {
        public const string AttachmentsStorageDirectoryName = "AttachmentsStorage";
        public const string ThumbnailPostfix = "_thumbnails";

        public const string BoardName = "Hikkaba";

        public const string DefaultAnonymousUserName = "Anonymous";
        public const string DefaultAnonymousEmail = "localhost@local.local";
        public const string DefaultAnonymousPassword = "KaRaSiQuE&%123";

        public const string DefaultAdminUserName = "Administrator";
        public const string DefaultAdminRoleName = "administrator";

        public const string DefaultMimeType = "application/octet-stream";
        public static string DefaultLastModified = new DateTime(2007, 01, 01).ToString("r"); // RFC1123

        public const string HtmlNewPostFormId = "NewPostForm";
    }
}
