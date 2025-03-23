using System.Net;
using System.Numerics;

namespace Hikkaba.Services.Implementations;

public static class IpRangeCalculator
{
    public static void GetHostRange(
        IPAddress networkAddress,
        int prefixLength,
        out IPAddress firstHost,
        out IPAddress lastHost)
    {
        byte[] addressBytes = networkAddress.GetAddressBytes();
        int totalBits = addressBytes.Length * 8;

        if (prefixLength < 0 || prefixLength > totalBits)
            throw new ArgumentException("Invalid prefix length for the given IP version.", nameof(prefixLength));

        // Convert IP address to BigInteger
        BigInteger ipValue = new BigInteger(addressBytes.Reverse().ToArray());

        // Calculate the subnet mask
        BigInteger mask = ((BigInteger.One << prefixLength) - 1) << (totalBits - prefixLength);
        BigInteger network = ipValue & mask;
        BigInteger broadcast = network | (~mask & ((BigInteger.One << totalBits) - 1));

        // Calculate the first and last usable host IPs (if more than 2 addresses in subnet)
        BigInteger first = (broadcast > network + 1) ? network + 1 : network;
        BigInteger last = (broadcast > network + 1) ? broadcast - 1 : broadcast;

        // Convert back to IPAddress
        byte[] firstBytes = first.ToByteArray();
        byte[] lastBytes = last.ToByteArray();

        firstHost = new IPAddress(PadBytes(firstBytes, addressBytes.Length));
        lastHost = new IPAddress(PadBytes(lastBytes, addressBytes.Length));
    }

    private static byte[] PadBytes(byte[] input, int length)
    {
        byte[] padded = new byte[length];
        for (int i = 0; i < length; i++)
        {
            padded[i] = (i < input.Length) ? input[i] : (byte)0;
        }
        Array.Reverse(padded);
        return padded;
    }
}
