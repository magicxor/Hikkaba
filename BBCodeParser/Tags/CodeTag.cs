using BBCodeParser.Enums;

namespace BBCodeParser.Tags;

public class CodeTag : Tag
{
    public CodeTag(string name, string openTag, string closeTag)
        : base(name, openTag, closeTag, true, AttributeEscapeMode.JsXss)
    {
    }
}
