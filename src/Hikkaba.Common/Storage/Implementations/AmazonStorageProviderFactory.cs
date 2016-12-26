using System;
using Hikkaba.Common.Storage.Interfaces;
using TwentyTwenty.Storage;

namespace Hikkaba.Common.Storage.Implementations
{
    public class AmazonStorageProviderFactory : IStorageProviderFactory
    {
        public IStorageProvider CreateStorageProvider()
        {
            throw new NotImplementedException();
            //return new AmazonStorageProvider(new AmazonProviderOptions
            //{
            //    Bucket = "mybucketname",
            //    PublicKey = "mypublickey",
            //    SecretKey = "mysecretkey"
            //});
        }
    }
}
