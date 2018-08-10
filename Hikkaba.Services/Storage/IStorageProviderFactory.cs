using TwentyTwenty.Storage;

namespace Hikkaba.Services.Storage
{
    public interface IStorageProviderFactory
    {
        IStorageProvider CreateStorageProvider();
    }
}