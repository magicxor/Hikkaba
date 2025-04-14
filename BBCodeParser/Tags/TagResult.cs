namespace BBCodeParser.Tags;

public class TagResult
{
    public Tag? Tag { get; set; }
    public required string Text { get; set; }
    public string? AttributeValue { get; set; }
    public string? Match { get; set; }
    public required TagType TagType { get; set; }
}
