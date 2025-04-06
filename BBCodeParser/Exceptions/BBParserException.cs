namespace BBCodeParser.Exceptions;

public class BBParserException : Exception
{
    public BBParserException()
    {
    }

    public BBParserException(string message)
        : base(message)
    {
    }

    public BBParserException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
