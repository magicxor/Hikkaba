namespace Hikkaba.Application.Contracts;

public interface IThreadLocalUserHashGenerator
{
    string Generate(string threadId, string? userIpAddress);
}
