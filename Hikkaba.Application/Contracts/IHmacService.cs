namespace Hikkaba.Application.Contracts;

public interface IHmacService
{
    byte[] HashHmac(byte[] key, byte[] message);
    byte[] HashHmac(string key, string message);
    public string GetTripCode(string input);
}
