using Bogus.DataSets;

namespace Hikkaba.Tests.Manual.Seed;

public static class BogusExtensions
{
    public static string Password(
        this Internet internet,
        int minLength,
        int maxLength,
        bool includeUppercase = true,
        bool includeNumber = true,
        bool includeSymbol = true)
    {
        ArgumentNullException.ThrowIfNull(internet);
        ArgumentOutOfRangeException.ThrowIfLessThan(minLength, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, minLength);

        var r = internet.Random;
        var s = string.Empty;

        s += r.Char('a', 'z').ToString();
        if (s.Length < maxLength && includeUppercase) s += r.Char('A', 'Z').ToString();
        if (s.Length < maxLength && includeNumber) s += r.Char('0', '9').ToString();
        if (s.Length < maxLength && includeSymbol) s += r.Char('!', '/').ToString();
        if (s.Length < minLength) s += r.String2(minLength - s.Length); // pad up to min
        if (s.Length < maxLength) s += r.String2(r.Number(0, maxLength - s.Length)); // random extra padding in range min..max

        var chars = s.ToArray();
        var charsShuffled = r.Shuffle(chars).ToArray();

        return new string(charsShuffled);
    }
}
