using System;
using System.Net;

namespace FooWebApp.Store
{
    public class StorageErrorException : Exception
    {
        public StorageErrorException(string message, Exception innerException, int statusCode) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public StorageErrorException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }
}
