namespace Hikkaba.Services.Contracts;

public interface ICryptoService
{
    string HashHmacHex(string key, string message);
    string HashHex(Stream inputStream);
}