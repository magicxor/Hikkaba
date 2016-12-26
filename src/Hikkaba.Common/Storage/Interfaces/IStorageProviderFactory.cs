using TwentyTwenty.Storage;

namespace Hikkaba.Common.Storage.Interfaces
{
    public interface IStorageProviderFactory
    {
        IStorageProvider CreateStorageProvider();
    }
}