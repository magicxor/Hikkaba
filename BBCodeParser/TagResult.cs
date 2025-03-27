namespace BBCodeParser;

public class TagResult
{
    public string Text { get; set; }
    public string RemainingInput { get; set; }
    public string ClosingTagValue { get; set; }
    public string OpeningTagValue { get; set; }
    public BBTag Tag { get; set; }
    public TagType TagType { get; set; }
}