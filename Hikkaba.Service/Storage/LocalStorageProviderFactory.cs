using TwentyTwenty.Storage;
using TwentyTwenty.Storage.Local;

namespace Hikkaba.Service.Storage
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
