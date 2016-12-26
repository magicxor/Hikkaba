using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Storage.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using TwentyTwenty.Storage;

namespace Hikkaba.Web.Controllers.Api
{
    public class AttachmentsController : Controller
    {
        private readonly IContentTypeProvider _contentTypeProvider = new FileExtensionContentTypeProvider();
        private readonly IStorageProvider _storageProvider;

        public AttachmentsController(IStorageProviderFactory storageProviderFactory)
        {
            _storageProvider = storageProviderFactory.CreateStorageProvider();
        }

        private string GetContentTypeByFileName(string fileName)
        {
            string contentType;
            if (_contentTypeProvider.TryGetContentType(fileName, out contentType))
            {
                return contentType;
            }
            else
            {
                return Defaults.DefaultMimeType;
            }
        }

        [ResponseCache(NoStore = false, Location = ResponseCacheLocation.Any, Duration = 31556926)]
        public async Task<FileStreamResult> Get(string containerName, string blobName, string fileExtension, bool getThumbnail)
        {
            if (getThumbnail)
            {
                containerName = containerName + Defaults.ThumbnailPostfix;
            }
            var fileName = blobName + "." + fileExtension;
            var contentType = GetContentTypeByFileName(fileName);
            var blobStream = await _storageProvider.GetBlobStreamAsync(containerName, blobName);
            return new FileStreamResult(blobStream, contentType);
        }
    }
}