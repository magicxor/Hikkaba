namespace Hikkaba.Application.Contracts;

public interface IHmacService
{
    string HashHmacHex(string key, string message);
}
