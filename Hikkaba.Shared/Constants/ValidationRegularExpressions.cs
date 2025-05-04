using System.Diagnostics.CodeAnalysis;

namespace Hikkaba.Shared.Constants;

public static class ValidationRegularExpressions
{
    [StringSyntax(StringSyntaxAttribute.Regex)]
    public const string LowercaseLatinChars = "^[a-z]*$";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    public const string LowercaseLatinCharsDigitsUnderscore = "^[a-z0-9_]*$";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    public const string UsernameField = @"(^[\p{L}\p{N}\p{P}]{0,100}##[\p{L}\p{N}\p{P}]{8,100}$)|(^(?!.*[#!])[\p{L}\p{N}\p{P}]*$)";
}
