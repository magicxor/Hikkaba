using System;
using TwentyTwenty.Storage;

namespace Hikkaba.Service.Storage
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
