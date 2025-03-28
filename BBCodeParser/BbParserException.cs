namespace BBCodeParser;

public class BbParserException : Exception
{
    public BbParserException() : base("Tree is too deep")
    {
    }
}
