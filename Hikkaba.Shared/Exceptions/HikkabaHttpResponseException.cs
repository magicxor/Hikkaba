using System;
using System.Net;

namespace Hikkaba.Shared.Exceptions;

public class HikkabaHttpResponseException
    : Exception
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public HikkabaHttpResponseException()
    {
    }

    public HikkabaHttpResponseException(string message)
        : base(message)
    {
    }

    public HikkabaHttpResponseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public HikkabaHttpResponseException(HttpStatusCode httpStatusCode)
    {
        HttpStatusCode = httpStatusCode;
    }

    public HikkabaHttpResponseException(HttpStatusCode httpStatusCode, string message)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
    }
}
