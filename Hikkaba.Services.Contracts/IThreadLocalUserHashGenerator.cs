namespace Hikkaba.Services.Contracts;

public interface IThreadLocalUserHashGenerator
{
    string Generate(string threadId, string? userIpAddress);
}
