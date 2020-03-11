using FooWebApp.Controllers;
using FooWebApp.DataContracts;
using FooWebApp.Store;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FooWebApp.Tests
{
    public class StudentsControllerTests
    {
        private Student _testStudent = new Student
        {
            Id = "123",
            Name = "John Smith",
            GradePercentage = 99
        };

        [Fact]
        public async Task AddStudentReturns503WhenStorageIsDown()
        {
            var studentsStoreMock = new Mock<IStudentStore>();
            //var loggerStub = new StudentsControllerLoggerStub();
            studentsStoreMock.Setup(store => store.AddStudent(_testStudent)).ThrowsAsync(new StorageErrorException());

            var controller = new StudentsController(studentsStoreMock.Object, Mock.Of<ILogger<StudentsController>>(), new TelemetryClient());

            IActionResult result = await controller.Post(_testStudent);
            AssertUtils.HasStatusCode(HttpStatusCode.ServiceUnavailable, result);

            // check that logs contain at least one error
            //Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        [Fact]
        public async Task AddStudentReturns500WhenExceptionIsNotKnown()
        {
            var studentsStoreMock = new Mock<IStudentStore>();
            studentsStoreMock.Setup(store => store.AddStudent(_testStudent)).ThrowsAsync(new Exception("Test Exception"));
            var loggerStub = new StudentsControllerLoggerStub();

            var controller = new StudentsController(studentsStoreMock.Object, loggerStub, new TelemetryClient());

            IActionResult result = await controller.Post(_testStudent);
            AssertUtils.HasStatusCode(HttpStatusCode.InternalServerError, result);

            // check that logs contain at least one error
            Assert.Contains(LogLevel.Error, loggerStub.LogEntries.Select(entry => entry.Level));
        }

        //TODO: complete the tests for the other controller methods
    }
}
