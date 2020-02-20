﻿using System;

namespace FooWebApp.Store
{
    public class StorageErrorException : Exception
    {
        public StorageErrorException()
        {
        }

        public StorageErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}