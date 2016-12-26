using Hikkaba.Common.Storage.Interfaces;
using TwentyTwenty.Storage;
using TwentyTwenty.Storage.Local;

namespace Hikkaba.Common.Storage.Implementations
{
    public class LocalStorageProviderFactory : IStorageProviderFactory
    {
        private readonly IStoragePathProvider _storagePathProvider;

        public LocalStorageProviderFactory(IStoragePathProvider storagePathProvider)
        {
            _storagePathProvider = storagePathProvider;
        }

        public IStorageProvider CreateStorageProvider()
        {
            return new LocalStorageProvider(_storagePathProvider.GetPath());
        }
    }
}
