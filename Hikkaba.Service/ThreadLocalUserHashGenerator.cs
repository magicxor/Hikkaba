namespace Hikkaba.Service
{
    public interface IThreadLocalUserHashGenerator
    {
        string Generate(string threadId, string userIpAddress);
    }

    public class ThreadLocalUserHashGenerator: IThreadLocalUserHashGenerator
    {
        private readonly ICryptoService _cryptoService;

        public ThreadLocalUserHashGenerator(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public string Generate(string threadId, string userIpAddress)
        {
            return _cryptoService.HashHmacHex(threadId, userIpAddress);
        }
    }
}
