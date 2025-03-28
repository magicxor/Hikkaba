namespace Hikkaba.Services.Contracts;

public interface IHashService
{
    byte[] GetHashBytes(Stream inputStream);
    byte[] GetHashBytes(string input);
    string GetHashHex(Stream inputStream);
    string GetHashHex(string input);
}
