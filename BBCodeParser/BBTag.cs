using System.Globalization;
using System.Text.RegularExpressions;
using BBCodeParser.Enums;

namespace BBCodeParser;

public class BBTag
{
    public BBTag(string bbTag, string openingHtmlTag, string closingHtmlTag,
        Dictionary<string, string>? attributes = null, bool selfAttributed = false)
    {
        BbTag = bbTag;
        OpeningHtmlTag = openingHtmlTag;
        ClosingHtmlTag = closingHtmlTag;
        RequiresClosingTag = true;
        Attributes = attributes;
        SelfAttributed = selfAttributed;
    }

    public BBTag(string bbTag, string openingHtmlTag,
        Dictionary<string, string>? attributes = null, bool selfAttributed = false)
    {
        BbTag = bbTag;
        OpeningHtmlTag = openingHtmlTag;
        RequiresClosingTag = false;
        Attributes = attributes;
        SelfAttributed = selfAttributed;
    }

    public string BbTag { get; set; }
    public bool RequiresClosingTag { get; set; }

    private Dictionary<string, string>? Attributes { get; set; }
    private bool SelfAttributed { get; set; }
    private string OpeningHtmlTag { get; set; }
    private string? ClosingHtmlTag { get; set; }

    public string GetOpenBbTagPattern(DirectionMode directionMode)
    {
        var attributeFormatPattern = directionMode == DirectionMode.BBToHtml
            ? "(?<{0}>.*?)"
            : "${{{0}}}";

        return string.Format(CultureInfo.InvariantCulture, @"{0}[{1}{2}]",
            directionMode == DirectionMode.BBToHtml ? @"\" : string.Empty,
            SelfAttributed
                ? string.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\"",
                    BbTag,
                    string.Format(CultureInfo.InvariantCulture, attributeFormatPattern, BbTag))
                : BbTag,
            Attributes == null || Attributes.Count == 0
                ? string.Empty
                : string.Format(CultureInfo.InvariantCulture, " {0}",
                    string.Join(" ", Attributes.Select(k => string.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\"",
                        k.Key,
                        string.Format(CultureInfo.InvariantCulture, attributeFormatPattern, k.Key))))));
    }

    public string GetCloseBbTagPattern(DirectionMode directionMode)
    {
        return directionMode == DirectionMode.BBToHtml
            ? string.Format(CultureInfo.InvariantCulture, @"\[\/{0}]", BbTag)
            : string.Format(CultureInfo.InvariantCulture, "[/{0}]", BbTag);
    }

    public string GetOpenHtmlTagPattern(DirectionMode directionMode)
    {
        var attributeFormatPattern = directionMode == DirectionMode.HtmlToBB
            ? "(?<{0}>.*?)"
            : "${{{0}}}";

        if ((Attributes == null || Attributes.Count == 0) && !SelfAttributed)
            return OpeningHtmlTag;

        var result = OpeningHtmlTag;

        if (SelfAttributed)
        {
            var regex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\{{{0}}}", BbTag));
            result = regex.Replace(result, string.Format(CultureInfo.InvariantCulture, attributeFormatPattern, BbTag));
        }

        if (Attributes != null && Attributes.Count > 0)
        {
            foreach (var attribute in Attributes)
            {
                var regex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\{{{0}}}", attribute.Key));
                result = regex.Replace(result, string.Format(CultureInfo.InvariantCulture, attributeFormatPattern, attribute.Key));
            }
        }

        return result;
    }

    public string? GetCloseHtmlTagPattern()
    {
        return ClosingHtmlTag;
    }
}
