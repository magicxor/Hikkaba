namespace BBCodeParser.Tags;

public class ListTag : Tag
{
    public ListTag(string name, string openTag, string closeTag, bool withAttribute = false, AttributeEscapeMode attributeEscape = AttributeEscapeMode.JsXss)
        : base(name, openTag, closeTag, withAttribute, attributeEscape)
    {
    }

    public ListTag(string name, string openTag, bool withAttribute = false, AttributeEscapeMode attributeEscape = AttributeEscapeMode.JsXss)
        : base(name, openTag, withAttribute, attributeEscape)
    {
    }
}
