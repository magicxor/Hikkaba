using TwentyTwenty.Storage;

namespace Hikkaba.Service.Storage
{
    public interface IStorageProviderFactory
    {
        IStorageProvider CreateStorageProvider();
    }
}