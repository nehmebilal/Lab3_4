using FooWebApp.Client;
using FooWebApp.DataContracts;
using FooWebApp.Store;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FooWebApp.Tests
{
    public class CoursesControllerTests
    {
        private Student _testStudent = new Student
        {
            Id = "123",
            Name = "John Smith",
            GradePercentage = 99
        };
        private string _courseName = "TestCourse";

        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.Conflict)]
        public async Task AddStudentReturnsCorrectStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            var studentsStoreMock = new Mock<IStudentStore>();
            studentsStoreMock.Setup(store => store.AddStudent(_courseName, _testStudent))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int)statusCode));

            TestServer testServer = new TestServer(
                Program.CreateWebHostBuilder(new string[] { })
                .ConfigureTestServices(services =>
                {
                    services.AddSingleton(studentsStoreMock.Object);
                }).UseEnvironment("Development"));
            var httpClient = testServer.CreateClient();
            var fooServiceClient = new FooServiceClient(httpClient);

            FooServiceException e = await Assert.ThrowsAsync<FooServiceException>(() => fooServiceClient.AddStudent(_courseName, _testStudent));
            Assert.Equal(statusCode, e.StatusCode);
        }
    }
}
