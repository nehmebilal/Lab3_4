using FooWebApp.Client;
using FooWebApp.DataContracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FooWebApp.IntegrationTests
{
    public class StudentsControllerIntegrationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
    {
        private readonly IFooServiceClient _fooServiceClient;
        private readonly Random _rand = new Random();

        // a concurrent container where we keep the records that were added to storage so that we can cleanup after
        private readonly ConcurrentBag<Student> _studentsToCleanup = new ConcurrentBag<Student>();

        public StudentsControllerIntegrationTests(IntegrationTestFixture fixture)
        {
            _fooServiceClient = fixture.FooServiceClient;
        }

        // implementation of IAsyncLifetime interface called by xunit
        public Task InitializeAsync()
        {
            // nothing to initialize
            return Task.CompletedTask;
        }

        // implementation of IAsyncLifetime interface called by xunit
        public async Task DisposeAsync()
        {
            var tasks = new List<Task>();
            foreach(var student in _studentsToCleanup)
            {
                // the task will be started but not initiated
                var task = _fooServiceClient.DeleteStudent(student.Id);

                // add the task to a list
                tasks.Add(task);
            }
            // await for all the tasks to complete. The main advantage of this approach is that the tasks
            // will run in parallel.
            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task PostGetStudent()
        {
            
            var student = CreateRandomStudent();
            await AddStudent(student);

            var fetchedStudent = await _fooServiceClient.GetStudent(student.Id);
            Assert.Equal(student, fetchedStudent);
        }
        
        [Theory]
        [InlineData("", "Joe", 10)]
        [InlineData(" ", "Joe", 10)]
        [InlineData(null, "Joe", 10)]
        [InlineData("123", "", 10)]
        [InlineData("123", " ", 10)]
        [InlineData("123", null, 10)]
        [InlineData("123", "Joe", -1)]
        [InlineData("123", "Joe", 101)]
        public async Task PostInvalidStudent(string id, string name, int grade)
        {
            var student = new Student
            {
                Name = name,
                Id = id,
                GradePercentage = grade
            };
            var e = await Assert.ThrowsAsync<FooServiceException>(() => _fooServiceClient.AddStudent(student));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingStudent()
        {
            string randomId = CreateRandomString();
            var e = await Assert.ThrowsAsync<FooServiceException>(() => _fooServiceClient.GetStudent(randomId));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }
        
        [Fact]
        public async Task AddStudentThatAlreadyExists()
        {
            var student = CreateRandomStudent();
            await AddStudent(student);
            
            var e = await Assert.ThrowsAsync<FooServiceException>(() => AddStudent(student));
            Assert.Equal(HttpStatusCode.Conflict, e.StatusCode);
        }

        [Fact]
        public async Task GetStudents()
        {
            var students = new List<Student>();
            var tasks = new List<Task>();
            for (int i = 0; i < 5; ++i)
            {
                Student student = CreateRandomStudent();
                students.Add(student);
                tasks.Add(AddStudent(student));
            }
            await Task.WhenAll(tasks); // add all the students in parallel

            GetStudentsResponse response = await _fooServiceClient.GetStudents();
            
            // check that all the students we added are there
            foreach (var student in students)
            {
                Assert.Contains(student, response.Students);
            }
            
            // check that the list of students in sorted
            Assert.True(IsSortedInDecreasedOrderOrGrades(response.Students));
        }

        [Fact]
        public async Task UpdateExistingStudent()
        {
            var student = CreateRandomStudent();
            await AddStudent(student);

            student.Name = CreateRandomString();
            student.GradePercentage = RandomGrade();
            await _fooServiceClient.UpdateStudent(student);
            var fetchedStudent = await _fooServiceClient.GetStudent(student.Id);
            Assert.Equal(student, fetchedStudent);
        }

        [Fact]
        public async Task UpdateNonExistingStudent()
        {
            var student = CreateRandomStudent();
            await _fooServiceClient.UpdateStudent(student);
            _studentsToCleanup.Add(student);

            // make sure the student was added, even though it did not exist
            var fetchedStudent = await _fooServiceClient.GetStudent(student.Id);
            Assert.Equal(student, fetchedStudent);
        }

        [Fact]
        public async Task DeleteStudent()
        {
            var student = CreateRandomStudent();
            await _fooServiceClient.AddStudent(student);
            await _fooServiceClient.DeleteStudent(student.Id);
            var e = await Assert.ThrowsAsync<FooServiceException>(() => _fooServiceClient.GetStudent(student.Id));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingStudent()
        {
            var student = CreateRandomStudent();
            var e = await Assert.ThrowsAsync<FooServiceException>(() => _fooServiceClient.DeleteStudent(student.Id));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        private async Task AddStudent(Student student)
        {
            await _fooServiceClient.AddStudent(student);
            _studentsToCleanup.Add(student);
        }

        private static bool IsSortedInDecreasedOrderOrGrades(List<Student> students)
        {
            for (int i = 0; i < students.Count - 1; i++)
            {
                if (students[i].GradePercentage < students[i+1].GradePercentage)
                {
                    return false;
                }
            }
            return true;
        }

        private static string CreateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        private Student CreateRandomStudent()
        {
            string studentId = CreateRandomString();
            var student = new Student
            {
                Id = studentId,
                Name = "Joe",
                GradePercentage = RandomGrade()
            };
            return student;
        }

        private int RandomGrade()
        {
            return _rand.Next(0, 100);
        }
    }
}
