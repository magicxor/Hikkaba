using System.Text.RegularExpressions;

namespace BBCodeParser;

public class MatchingTag
{
    public MatchingTag()
    {
        TagType = TagType.NoResult;
    }

    public Match? Match { get; set; }
    public BBTag? Tag { get; set; }
    public TagType TagType { get; set; }
}
