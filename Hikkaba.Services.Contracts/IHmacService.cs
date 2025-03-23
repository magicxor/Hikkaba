namespace Hikkaba.Services.Contracts;

public interface IHmacService
{
    string HashHmacHex(string key, string message);
}
