namespace Hikkaba.Application.Contracts;

public interface IHashService
{
    byte[] GetHashBytes(Stream inputStream);
    byte[] GetHashBytes(string input);
    byte[] GetHashBytes(byte[] input);
    public byte[] GetHashBytes(Guid threadSalt, byte[] userIp);
    string GetHashHex(Stream inputStream);
    string GetHashHex(string input);
    string GetHashHex(byte[] input);
    string GetHashHex(Guid threadSalt, byte[] userIp);
}
