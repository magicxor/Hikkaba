using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BBCodeParser;

public class HtmlToBBReadingStrategy : IReadingStrategy
{
    private readonly IEnumerable<BBTag> _tags;

    public HtmlToBBReadingStrategy(IEnumerable<BBTag> tags)
    {
        this._tags = tags;
    }

    public TagResult Read(string input)
    {
        var firstMatchingTag = GetFirstMatchingTag(input);
        if (firstMatchingTag.TagType == TagType.Open)
        {
            var value = firstMatchingTag.Match.Result(firstMatchingTag.Tag.GetOpenBbTagPattern(DirectionMode.HtmlToBB));
            var remainingInput = input.Substring(firstMatchingTag.Match.Value.Length + firstMatchingTag.Match.Index);
            var text = input.Substring(0, firstMatchingTag.Match.Index);
            return new TagResult
            {
                Text = text,
                OpeningTagValue = value,
                ClosingTagValue = firstMatchingTag.Tag.RequiresClosingTag ? firstMatchingTag.Tag.GetCloseBbTagPattern(DirectionMode.HtmlToBB) : null,
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
        foreach (var bbTag in _tags)
        {
            var openTagRegex = new Regex(bbTag.GetOpenHtmlTagPattern(DirectionMode.HtmlToBB));
            var openTagMatch = openTagRegex.Match(input);
            if (openTagMatch.Success && openTagMatch.Index < minIndex)
            {
                minIndex = openTagMatch.Index;
                result.Tag = bbTag;
                result.TagType = TagType.Open;
                result.Match = openTagMatch;
            }

            if (!bbTag.RequiresClosingTag) continue;

            var closeTagRegex = new Regex(bbTag.GetCloseHtmlTagPattern());
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
