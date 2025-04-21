using System.Text.RegularExpressions;

namespace Hikkaba.Infrastructure.Repositories.Utils;

public static partial class FullTextUtils
{
    [GeneratedRegex("""[\s\"\*]+""", RegexOptions.Compiled, 500)]
    private static partial Regex GetTokenDelimitersRegex();

    private static readonly Regex TokenDelimitersRegex = GetTokenDelimitersRegex();

    public static string ConvertToSearchableString(string? userInput)
    {
        userInput = userInput?.Trim();

        if (string.IsNullOrEmpty(userInput))
            return string.Empty;

        var quotedTokens = TokenDelimitersRegex
            .Split(userInput)
            .Select(token => token.Trim())
            .Where(token => !string.IsNullOrEmpty(token))
            .Select(token => $"\"{token}\"");

        return string.Join(" AND ", quotedTokens);
    }
}
