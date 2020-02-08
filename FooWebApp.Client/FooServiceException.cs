using System;
using System.Net;

namespace FooWebApp.Client
{
    public class FooServiceException : Exception
    {
        public FooServiceException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
