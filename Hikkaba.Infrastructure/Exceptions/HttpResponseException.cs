using System;
using System.Net;

namespace Hikkaba.Infrastructure.Exceptions;

public class HttpResponseException: Exception
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public HttpResponseException(HttpStatusCode httpStatusCode)
    {
        HttpStatusCode = httpStatusCode;
    }

    public HttpResponseException(HttpStatusCode httpStatusCode, string message) : base(message)
    {
        HttpStatusCode = httpStatusCode;
    }
}