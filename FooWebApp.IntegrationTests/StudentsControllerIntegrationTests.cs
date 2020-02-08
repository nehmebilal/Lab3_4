using FooWebApp.Client;
using FooWebApp.DataContracts;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FooWebAppIntegrationTests
{
    public class StudentsControllerIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IFooServiceClient _fooServiceClient;

        public StudentsControllerIntegrationTests(IntegrationTestFixture fixture)
        {
            _fooServiceClient = fixture.FooServiceClient;
        }

        [Fact]
        public async Task PostGetStudent()
        {
            string studentId = CreateRandomId();
            var student = new Student
            {
                Id = studentId,
                Name = "Joe",
                GradePercentage = 99
            };

            await _fooServiceClient.AddStudent(student);

            var fetchedStudent = await _fooServiceClient.GetStudent(studentId);
            Assert.Equal(student, fetchedStudent);
        }

        [Fact]
        public async Task NotFoundIsReturnedWhenStudentIsNotInStorage()
        {
            string randomId = CreateRandomId();
            var e = await Assert.ThrowsAsync<FooServiceException>(() => _fooServiceClient.GetStudent(randomId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        private static string CreateRandomId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
