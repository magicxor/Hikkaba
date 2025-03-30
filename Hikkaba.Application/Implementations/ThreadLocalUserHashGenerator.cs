using Hikkaba.Application.Contracts;

namespace Hikkaba.Application.Implementations;

public class ThreadLocalUserHashGenerator: IThreadLocalUserHashGenerator
{
    private readonly IHmacService _hmacService;

    public ThreadLocalUserHashGenerator(IHmacService hmacService)
    {
        _hmacService = hmacService;
    }

    public string Generate(string threadId, string? userIpAddress)
    {
        if (userIpAddress == null)
        {
            return string.Empty;
        }

        return _hmacService.HashHmacHex(threadId, userIpAddress);
    }
}
