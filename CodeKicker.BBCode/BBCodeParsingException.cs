using System;

namespace CodeKicker.BBCode;

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
