using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hikkaba.Common.Exceptions
{
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
}
