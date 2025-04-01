namespace BBCodeParser;

public class BBParserException : Exception
{
    public BBParserException() : base("Tree is too deep")
    {
    }
}
