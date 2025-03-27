using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BBCodeParser;

public class BBToHtmlReadingStrategy : IReadingStrategy
{
    private readonly IEnumerable<BBTag> tags;

    public BBToHtmlReadingStrategy(IEnumerable<BBTag> tags)
    {
        this.tags = tags;
    }

    public TagResult Read(string input)
    {
        var firstMatchingTag = GetFirstMatchingTag(input);
        if (firstMatchingTag.TagType == TagType.Open)
        {
            var value = firstMatchingTag.Match.Result(firstMatchingTag.Tag.GetOpenHtmlTagPattern(DirectionMode.BBToHtml));
            var remainingInput = input.Substring(firstMatchingTag.Match.Value.Length + firstMatchingTag.Match.Index);
            var text = input.Substring(0, firstMatchingTag.Match.Index);
            return new TagResult
            {
                Text = text,
                OpeningTagValue = value,
                ClosingTagValue = firstMatchingTag.Tag.RequiresClosingTag ? firstMatchingTag.Tag.GetCloseHtmlTagPattern() : null,
                RemainingInput = remainingInput,
                Tag = firstMatchingTag.Tag,
                TagType = TagType.Open
            };
        }
        if (firstMatchingTag.TagType == TagType.Close)
        {
            var text = input.Substring(0, firstMatchingTag.Match.Index);
            var remainingInput = input.Substring(firstMatchingTag.Match.Value.Length + firstMatchingTag.Match.Index);
            return new TagResult
            {
                Text = text,
                RemainingInput = remainingInput,
                Tag = firstMatchingTag.Tag,
                TagType = TagType.Close
            };
        }
        return new TagResult
        {
            Text = input,
            TagType = TagType.NoResult
        };
    }

    private MatchingTag GetFirstMatchingTag(string input)
    {
        var minIndex = input.Length;
        var result = new MatchingTag();
        foreach (var bbTag in tags)
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