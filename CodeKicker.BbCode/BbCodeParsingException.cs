using System;

namespace CodeKicker.BbCode;

[Serializable]
public class BbCodeParsingException : Exception
{
    public BbCodeParsingException()
    {
    }

    public BbCodeParsingException(string message)
        : base(message)
    {
    }
}
