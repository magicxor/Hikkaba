namespace Hikkaba.Application.Contracts;

public interface ICryptoService
{
    string HashHmacHex(string key, string message);
    string HashHex(Stream inputStream);
}