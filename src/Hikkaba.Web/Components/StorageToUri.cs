using Hikkaba.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Components
{
    public class StorageToUri : ViewComponent
    {
        public string Invoke(string containerName, string blobName, bool getThumbnail = false)
        {
            if (getThumbnail)
            {
                containerName = containerName + Defaults.ThumbnailPostfix;
            }
            return "/" + Defaults.AttachmentsStorageDirectoryName + "/" + containerName + "/" + blobName;
        }
    }
}