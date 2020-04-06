using FooWebApp.DataContracts;
using FooWebApp.Exceptions;
using FooWebApp.Store;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FooWebApp.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentStore _studentStore;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IStudentStore studentStore, TelemetryClient telemetryClient, ILogger<StudentService> logger)
        {
            _studentStore = studentStore;
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        public async Task<Student> GetStudent(string courseName, string studentId)
        {
            using (_logger.BeginScope("{CourseName}, {StudentId}", courseName, studentId))
            {
                var stopWatch = Stopwatch.StartNew();
                Student student = await _studentStore.GetStudent(courseName, studentId);
                _telemetryClient.TrackMetric("StudentStore.GetStudent.Time", stopWatch.ElapsedMilliseconds);
                return student;
            }
        }

        public async Task AddStudent(string courseName, Student student)
        {
            using (_logger.BeginScope("{CourseName}, {StudentId}", courseName, student.Id))
            {
                ThrowBadRequestIfStudentInvalid(student);

                var stopWatch = Stopwatch.StartNew();
                await _studentStore.AddStudent(courseName, student);

                _telemetryClient.TrackMetric("StudentStore.AddStudent.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("StudentAdded");
            }
        }

        public async Task DeleteStudent(string courseName, string studentId)
        {
            using (_logger.BeginScope("{CourseName}, {StudentId}", courseName, studentId))
            {
                var stopWatch = Stopwatch.StartNew();
                await _studentStore.DeleteStudent(courseName, studentId);
                _telemetryClient.TrackMetric("StudentStore.DeleteStudent.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("StudentDeleted");
            }
        }

        public async Task<GetStudentsResult> GetStudents(string courseName, string continuationToken, int limit)
        {
            using (_logger.BeginScope("{CourseName}", courseName))
            {
                var stopWatch = Stopwatch.StartNew();
                GetStudentsResult result = await _studentStore.GetStudents(courseName, continuationToken, limit);
                _telemetryClient.TrackMetric("StudentStore.GetStudents.Time", stopWatch.ElapsedMilliseconds);
                return result;
            }
        }

        public async Task UpdateStudent(string courseName, Student student)
        {
            using (_logger.BeginScope("{CourseName}, {StudentId}", courseName, student.Id))
            {
                ThrowBadRequestIfStudentInvalid(student);

                var stopWatch = Stopwatch.StartNew();
                await _studentStore.UpdateStudent(courseName, student);
                _telemetryClient.TrackMetric("StudentStore.UpdateStudents.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("StudentUpdated");
            }
        }

        private void ThrowBadRequestIfStudentInvalid(Student student)
        {
            string error = null;
            if (string.IsNullOrWhiteSpace(student.Id))
            {
                error = "The id must not be empty";
            }
            if (string.IsNullOrWhiteSpace(student.Name))
            {
                error = "The Name must not be empty";
            }
            if (student.GradePercentage < 0 || student.GradePercentage > 100)
            {
                error = $"GradePercentage {student.GradePercentage} is not valid";
            }

            if (error != null)
            {
                throw new BadRequestException(error);
            }
        }
    }
}
