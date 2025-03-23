using System;

namespace Hikkaba.Common.Exceptions;

public class HikkabaConfigException : Exception
{
    public HikkabaConfigException()
    {
    }

    public HikkabaConfigException(string message) : base(message)
    {
    }

    public HikkabaConfigException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
