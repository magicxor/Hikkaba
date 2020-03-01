using System.IO;
using Hikkaba.Common.Constants;
using Hikkaba.Services.Storage;
using Microsoft.AspNetCore.Hosting;

namespace Hikkaba.Web.Services
{
    public class LocalStoragePathProvider: IStoragePathProvider
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public LocalStoragePathProvider(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public string GetPath()
        {
            return Path.Combine(_hostingEnvironment.WebRootPath, Defaults.AttachmentsStorageDirectoryName);
        }
    }
}
