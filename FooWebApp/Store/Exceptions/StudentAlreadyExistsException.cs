using System;

namespace FooWebApp.Store
{
    public class StudentAlreadyExistsException : Exception
    {
        public StudentAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
