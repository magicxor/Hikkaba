using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Hikkaba.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using TwentyTwenty.Storage;

namespace Hikkaba.Web.Controllers.Api;

[ApiController]
[AllowAnonymous]
[Route("api/v1/attachments")]
public sealed class AttachmentsController : ControllerBase
{
    private readonly IStorageProvider _storageProvider;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    public AttachmentsController(
        IStorageProvider storageProvider,
        FileExtensionContentTypeProvider contentTypeProvider)
    {
        _storageProvider = storageProvider;
        _contentTypeProvider = contentTypeProvider;
    }

    private string GetContentTypeByFileName(string fileName)
    {
        return _contentTypeProvider.TryGetContentType(fileName, out var contentType) ? contentType : Defaults.DefaultMimeType;
    }

    [HttpGet("{blobContainerId}/{blobId}", Name = "AttachmentsGet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ResponseCache(NoStore = false, Location = ResponseCacheLocation.Any, Duration = Defaults.DefaultAttachmentsCacheDuration)]
    public async Task<IActionResult> Get(
        [Required] [FromRoute] [MaxLength(Defaults.MaxGuidLength)] string blobContainerId,
        [Required] [FromRoute] [MaxLength(Defaults.MaxGuidLength)] string blobId,
        [Required] [FromQuery] [MaxLength(Defaults.MaxFileExtensionLength)] string fileExtension,
        [FromQuery] bool getThumbnail)
    {
        HttpContext.Response.Headers[HeaderNames.LastModified] = Defaults.DefaultLastModified;

        if (HttpContext.Request.Headers.ContainsKey(HeaderNames.IfModifiedSince))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var fileName = blobId + "." + fileExtension;
        if (getThumbnail)
        {
            blobId += Defaults.ThumbnailPostfix;
        }
        var contentType = GetContentTypeByFileName(fileName);
        var blobDescriptor = await _storageProvider.GetBlobDescriptorAsync(blobContainerId, blobId);
        var blobStream = await _storageProvider.GetBlobStreamAsync(blobContainerId, blobId);

        HttpContext.Response.Headers[HeaderNames.ContentDisposition] = "inline; filename=" + fileName;
        HttpContext.Response.ContentType = contentType;
        HttpContext.Response.ContentLength = blobDescriptor.Length;

        return new FileStreamResult(blobStream, contentType);
    }
}
