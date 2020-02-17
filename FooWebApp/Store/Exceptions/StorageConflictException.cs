using System;

namespace FooWebApp.Store
{
    public class StorageConflictException : Exception
    {
        public StorageConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
