using System;

namespace Hikkaba.Shared.Exceptions;

public class HikkabaDataException : Exception
{
    public HikkabaDataException()
    {
    }

    public HikkabaDataException(string message)
        : base(message)
    {
    }

    public HikkabaDataException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
