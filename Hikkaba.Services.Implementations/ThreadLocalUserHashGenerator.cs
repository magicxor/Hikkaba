using Hikkaba.Services.Contracts;

namespace Hikkaba.Services.Implementations;

public class ThreadLocalUserHashGenerator: IThreadLocalUserHashGenerator
{
    private readonly IHmacService _hmacService;

    public ThreadLocalUserHashGenerator(IHmacService hmacService)
    {
        _hmacService = hmacService;
    }

    public string Generate(string threadId, string userIpAddress)
    {
        return _hmacService.HashHmacHex(threadId, userIpAddress);
    }
}
