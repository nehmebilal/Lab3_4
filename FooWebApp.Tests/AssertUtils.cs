using Microsoft.AspNetCore.Mvc;
using System.Net;
using Xunit;

namespace FooWebApp.Tests
{
    public static class AssertUtils
    {
        /// <summary>
        /// Used to avoid duplicating this same code in several tests
        /// </summary>
        public static void HasStatusCode(HttpStatusCode statusCode, IActionResult actionResult)
        {
            Assert.True(actionResult is ObjectResult);
            ObjectResult objectResult = (ObjectResult)actionResult;

            Assert.Equal((int)statusCode, objectResult.StatusCode);
        }
    }
}
