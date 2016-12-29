using System;

namespace Hikkaba.Common.Constants
{
    public static class Defaults
    {
        public const string AttachmentsStorageDirectoryName = "AttachmentsStorage";
        public const string ThumbnailPostfix = "_thumbnails";

        public const string BoardName = "Hikkaba";

        public const string AnonymousUserName = "Anonymous";
        public const string AnonymousEmail = "localhost@local.local";
        public const string AnonymousPassword = "KaRaSiQuE&%123";

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
        public static string DefaultLastModified = new DateTime(2007, 01, 01).ToString("r"); // RFC1123

        public const string HtmlNewPostFormId = "NewPostForm";
    }
}
