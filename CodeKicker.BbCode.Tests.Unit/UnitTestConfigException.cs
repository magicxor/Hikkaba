using System;

namespace CodeKicker.BbCode.Tests.Unit;

public class UnitTestConfigException : Exception
{
    public UnitTestConfigException()
    {
    }

    public UnitTestConfigException(string message) : base(message)
    {
    }

    public UnitTestConfigException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
