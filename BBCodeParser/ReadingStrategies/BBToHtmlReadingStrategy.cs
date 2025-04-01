using System.Text.RegularExpressions;
using BBCodeParser.Enums;

namespace BBCodeParser.ReadingStrategies;

public class BBToHtmlReadingStrategy : IReadingStrategy
{
    private readonly IEnumerable<BBTag> _tags;

    public BBToHtmlReadingStrategy(IEnumerable<BBTag> tags)
    {
        this._tags = tags;
    }

    public TagResult Read(string input)
    {
        var firstMatchingTag = GetFirstMatchingTag(input);
        if (firstMatchingTag.TagType == TagType.Open)
        {
            var value = firstMatchingTag.Match?.Result(firstMatchingTag.Tag?.GetOpenHtmlTagPattern(DirectionMode.BBToHtml) ?? string.Empty);
            var remainingInput = input.Substring((firstMatchingTag.Match?.Value.Length ?? 0) + (firstMatchingTag.Match?.Index ?? 0));
            var text = input.Substring(0, firstMatchingTag.Match?.Index ?? 0);
            return new TagResult
            {
                Text = text,
                OpeningTagValue = value ?? string.Empty,
                ClosingTagValue = firstMatchingTag.Tag?.RequiresClosingTag == true ? firstMatchingTag.Tag.GetCloseHtmlTagPattern() : null,
                RemainingInput = remainingInput,
                Tag = firstMatchingTag.Tag,
                TagType = TagType.Open,
            };
        }
        if (firstMatchingTag.TagType == TagType.Close)
        {
            var text = input.Substring(0, firstMatchingTag.Match?.Index ?? 0);
            var remainingInput = input.Substring((firstMatchingTag.Match?.Value.Length ?? 0) + (firstMatchingTag.Match?.Index ?? 0));
            return new TagResult
            {
                Text = text,
                RemainingInput = remainingInput,
                Tag = firstMatchingTag.Tag,
                TagType = TagType.Close,
            };
        }
        return new TagResult
        {
            Text = input,
            TagType = TagType.NoResult,
        };
    }

    private MatchingTag GetFirstMatchingTag(string input)
    {
        var minIndex = input.Length;
        var result = new MatchingTag();
        foreach (var bbTag in _tags)
        {
            var openTagRegex = new Regex(bbTag.GetOpenBbTagPattern(DirectionMode.BBToHtml));
            var openTagMatch = openTagRegex.Match(input);
            if (openTagMatch.Success && openTagMatch.Index < minIndex)
            {
                minIndex = openTagMatch.Index;
                result.Tag = bbTag;
                result.TagType = TagType.Open;
                result.Match = openTagMatch;
            }

            if (!bbTag.RequiresClosingTag) continue;

            var closeTagRegex = new Regex(bbTag.GetCloseBbTagPattern(DirectionMode.BBToHtml));
            var closeTagMatch = closeTagRegex.Match(input);
            if (closeTagMatch.Success && closeTagMatch.Index < minIndex)
            {
                minIndex = closeTagMatch.Index;
                result.Tag = bbTag;
                result.TagType = TagType.Close;
                result.Match = closeTagMatch;
            }
        }

        return result;
    }
}
