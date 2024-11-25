using Hikkaba.Services.Contracts;

namespace Hikkaba.Services.Implementations;

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