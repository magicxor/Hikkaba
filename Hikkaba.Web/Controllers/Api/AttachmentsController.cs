using System.Threading.Tasks;
using Hikkaba.Common.Constants;
using Hikkaba.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
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
        
        [ResponseCache(NoStore = false, Location = ResponseCacheLocation.Any, Duration = Defaults.DefaultAttachmentsCacheDuration)]
        public async Task<IActionResult> Get(string containerName, string blobName, string fileExtension, bool getThumbnail)
        {
            HttpContext.Response.Headers[HeaderNames.LastModified] = Defaults.DefaultLastModified;
            
            if (HttpContext.Request.Headers.ContainsKey(HeaderNames.IfModifiedSince))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }
            else
            {
                var fileName = blobName + "." + fileExtension;
                if (getThumbnail)
                {
                    containerName = containerName + Defaults.ThumbnailPostfix;
                }
                var contentType = GetContentTypeByFileName(fileName);
                var blobDescriptor = await _storageProvider.GetBlobDescriptorAsync(containerName, blobName);
                var blobStream = await _storageProvider.GetBlobStreamAsync(containerName, blobName);
                
                HttpContext.Response.Headers[HeaderNames.ContentDisposition] = "inline; filename=" + fileName;
                HttpContext.Response.ContentType = contentType;
                HttpContext.Response.ContentLength = blobDescriptor.Length;

                return new FileStreamResult(blobStream, contentType);
            }
        }
    }
}