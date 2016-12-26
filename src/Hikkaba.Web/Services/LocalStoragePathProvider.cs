using System.IO;
using Hikkaba.Common.Constants;
using Hikkaba.Common.Storage.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Hikkaba.Web.Services
{
    public class LocalStoragePathProvider: IStoragePathProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public LocalStoragePathProvider(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public string GetPath()
        {
            return Path.Combine(_hostingEnvironment.WebRootPath, Defaults.AttachmentsStorageDirectoryName);
        }
    }
}
