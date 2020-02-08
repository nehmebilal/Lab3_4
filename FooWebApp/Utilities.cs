using System;

namespace FooWebApp
{
    public class Utilities
    {
        public static void CheckNullOrWhitespace(string paramName,string toCheck)
        {
            if (string.IsNullOrWhiteSpace(toCheck) || string.IsNullOrEmpty(toCheck))
            {
                throw new ArgumentException($"{paramName} is null or empty!");
            }
        }
    }
}