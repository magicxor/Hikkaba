using System.Text.RegularExpressions;

namespace BBCodeParser;

public static partial class UriUtility
{
    public static bool IsWellFormedUriString(string uriStr, UriKind uriKind)
    {
        // uri = ""
        if (string.IsNullOrWhiteSpace(uriStr))
            return false;

        // uri = "/"
        if (uriKind == UriKind.Relative && uriStr == "/")
            return true;

        // uri = "#anchor"
        if (uriKind == UriKind.Relative && ValidUriFragmentRegex().IsMatch(uriStr))
            return true;

        // other cases
        var successfullyCreated = Uri.TryCreate(uriStr, uriKind, out var uri);
        if (!successfullyCreated)
            return false;

        // uri = "//example.com"
        if (uriKind == UriKind.Relative
            && uriStr.StartsWith("//", StringComparison.Ordinal))
        {
            return true;
        }

        return ValidUriRegex().IsMatch(uriStr);
    }

    [GeneratedRegex("""
                    ^
                    (# Scheme
                     (?<scheme>[a-z][a-z0-9+\-.]*):
                     (# Authority & path
                      //
                      (?:(?<user>[a-z0-9\-._~%!$&'()*+,;=]+)(:(?<password>[a-z0-9\-._~%!$&'()*+,;=]+)?)?@)? # User and password
                      (?<host>[\p{L}\p{M}\p{N}\-.]+                      # Named host
                      |       \[[a-f0-9:.]+\]                            # IPv6 host
                      |       \[v[a-f0-9][a-z0-9\-._~%!$&'()*+,;=:]+\])  # IPvFuture host
                      (?<port>:[0-9]+)?                                  # Port
                      (?<path>(/[a-z0-9\-._~%!$&'()*+,;=:@]+)*/?)        # Path
                     |# Path without authority
                      (?<path>/?[a-z0-9\-._~%!$&'()*+,;=:@]+(/[a-z0-9\-._~%!$&'()*+,;=:@]+)*/?)?
                     )
                    |# Relative URL (no scheme or authority)
                     (?<path>[a-z0-9\-._~%!$&'()*+,;=@]+(/[a-z0-9\-._~%!$&'()*+,;=:@]+)*/?  # Relative path
                     |(/[a-z0-9\-._~%!$&'()*+,;=:@]+)+/?)                                   # Absolute path
                    )
                    # Query
                    (?<query>\?[a-z0-9\-._~%!$&'()*+,;=:@/?]*)?
                    # Fragment
                    (?<fragment>\#[a-z0-9\-._~%!$&'()*+,;=:@/?]*)?
                    $
                    """, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, 200)]
    private static partial Regex ValidUriRegex();

    [GeneratedRegex("""^\#[a-z0-9\-._~%!$&'()*+,;=:@/?]+$""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, 200)]
    private static partial Regex ValidUriFragmentRegex();
}
