using BBCodeParser.Enums;

namespace BBCodeParser.Tags;

public class PreformattedTag : Tag
{
    public PreformattedTag(string name, string openTag, string closeTag) : base(name, openTag, closeTag, false,
        AttributeEscapeMode.Html)
    {
    }
}
