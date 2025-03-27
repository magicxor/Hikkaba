using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BBCodeParser.Tags;

public partial class Tag
{
    private static readonly Regex JsXssSecureRegex = JsXssSecureRegexClass();

    private static readonly Regex[] EscapeRegexes =
    {
        EscapeRegex1(),
        EscapeRegex2(),
    };

    private string OpenTag { get; }
    private string CloseTag { get; }
    public bool WithAttribute { get; }
    public bool RequiresClosing { get; }
    private AttributeEscapeMode AttributeEscape { get; }
    public string Name { get; }

    public Tag(string name, string openTag, string closeTag, bool withAttribute = false, AttributeEscapeMode attributeEscape = AttributeEscapeMode.JsXss)
    {
        OpenTag = openTag;
        CloseTag = closeTag;
        WithAttribute = withAttribute;
        AttributeEscape = attributeEscape;
        RequiresClosing = true;
        Name = name;
    }

    public Tag(string name, string openTag, bool withAttribute = false, AttributeEscapeMode attributeEscape = AttributeEscapeMode.JsXss)
    {
        OpenTag = openTag;
        CloseTag = null;
        WithAttribute = withAttribute;
        AttributeEscape = attributeEscape;
        RequiresClosing = false;
        Name = name;
    }

    public string GetOpenHtml(string attributeValue)
    {
        return GetHtmlPart(OpenTag, attributeValue);
    }

    public string GetCloseHtml(string attributeValue)
    {
        return GetHtmlPart(CloseTag, attributeValue);
    }

    private string GetHtmlPart(string tagPart, string attributeValue)
    {
        if (attributeValue.Contains('"'))
        {
            attributeValue = attributeValue.Replace("\"", "&quot;", StringComparison.Ordinal);
        }

        return WithAttribute ? tagPart.Replace("{value}", GetAttributeValueForHtml(attributeValue)) : tagPart;
    }

    private string GetAttributeValueForHtml(string attributeValue)
    {
        if (attributeValue.Contains('"'))
        {
            attributeValue = attributeValue.Replace("\"", "&quot;", StringComparison.Ordinal);
        }

        return AttributeEscape switch
        {
            AttributeEscapeMode.JsXss => JsXssSecureRegex.Replace(EscapeSpecialCharacters(attributeValue), "_xss_"),
            AttributeEscapeMode.AbsoluteUri => JsXssSecureRegex.Replace(EncodeUrl(attributeValue, UriKind.Absolute), "_xss_"),
            AttributeEscapeMode.RelativeUri => JsXssSecureRegex.Replace(EncodeUrl(attributeValue, UriKind.Relative), "_xss_"),
            AttributeEscapeMode.Html => attributeValue.Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(attributeValue)),
        };
    }

    private static string EncodeUrl(string originalUrl, UriKind uriKind)
    {
        return UriUtility.IsWellFormedUriString(originalUrl, uriKind)
            ? originalUrl
            : HttpUtility.UrlEncode(originalUrl);
    }

    private static string EscapeSpecialCharacters(string value)
    {
        while (true)
        {
            var escaped = EscapeRegexes.Aggregate(value, (input, regex) => regex.Replace(input, string.Empty));
            if (escaped == value)
            {
                return escaped;
            }
            value = escaped;
        }
    }

    [GeneratedRegex("(javascript|data):", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking, 500)]
    private static partial Regex JsXssSecureRegexClass();

    [GeneratedRegex(@"""|'|`|\n|\s|\t|\r|\<|\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking, 500)]
    private static partial Regex EscapeRegex1();

    [GeneratedRegex(@"&#[\d\w]+;?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking, 500)]
    private static partial Regex EscapeRegex2();
}
