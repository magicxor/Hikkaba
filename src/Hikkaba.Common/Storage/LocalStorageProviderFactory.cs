using TwentyTwenty.Storage;
using TwentyTwenty.Storage.Local;

namespace Hikkaba.Common.Storage
{
    public interface ILocalStorageProviderFactory
    {
        IStorageProvider CreateLocalStorageProvider();
    }

    public class LocalStorageProviderFactory: ILocalStorageProviderFactory
    {
        private readonly IStoragePathProvider _storagePathProvider;

        public LocalStorageProviderFactory(IStoragePathProvider storagePathProvider)
        {
            _storagePathProvider = storagePathProvider;
        }

        public IStorageProvider CreateLocalStorageProvider()
        {
            return new LocalStorageProvider(_storagePathProvider.GetPath());
        }
    }
}
