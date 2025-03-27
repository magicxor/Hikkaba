namespace BBCodeParser.Tags;

public class TagResult
{
    public Tag Tag { get; set; }
    public string Text { get; set; }
    public string AttributeValue { get; set; }
    public string Match { get; set; }
    public TagType TagType { get; set; }
}