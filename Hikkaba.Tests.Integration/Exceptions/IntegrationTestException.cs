using System;

namespace Hikkaba.Tests.Integration.Exceptions;

internal sealed class IntegrationTestException : Exception
{
    public IntegrationTestException()
    {
    }

    public IntegrationTestException(string message)
        : base(message)
    {
    }
    
    public IntegrationTestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
