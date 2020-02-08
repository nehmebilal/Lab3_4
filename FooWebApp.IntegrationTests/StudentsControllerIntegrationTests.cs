using FooWebApp.Client;
using FooWebApp.DataContracts;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FooWebApp.IntegrationTests
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
            var student = CreateRandomStudent();
            await _fooServiceClient.AddStudent(student);

            var fetchedStudent = await _fooServiceClient.GetStudent(student.Id);
            Assert.Equal(student, fetchedStudent);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task AddStudentNameBadRequest(string studentName)
        {
            var student = new Student
            {
                Name = studentName,
                Id = CreateRandomString()
            };
            var e = await Assert.ThrowsAsync<FooServiceException>(() => _fooServiceClient.AddStudent(student));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task AddStudentIdBadRequest(string studentId)
        {
            var student = new Student
            {
                Id = studentId,
                Name = CreateRandomString()
            };
            var e = await Assert.ThrowsAsync<FooServiceException>(() => _fooServiceClient.AddStudent(student));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }

        [Fact]
        public async Task NotFoundIsReturnedWhenStudentIsNotInStorage()
        {
            string randomId = CreateRandomString();
            var e = await Assert.ThrowsAsync<FooServiceException>(() => _fooServiceClient.GetStudent(randomId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }
        
        [Fact]
        public async Task ConflictWhenStudentAlreadyExists()
        {
            var student = CreateRandomStudent();
            await _fooServiceClient.AddStudent(student);
            
            var e = await Assert.ThrowsAsync<FooServiceException>(() => _fooServiceClient.AddStudent(student));
            Assert.Equal(HttpStatusCode.Conflict, e.StatusCode);
        }

        private static string CreateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        private static Student CreateRandomStudent()
        {
            string studentId = CreateRandomString();
            var student = new Student
            {
                Id = studentId,
                Name = "Joe",
                GradePercentage = 99
            };
            return student;
        }
    }
}
