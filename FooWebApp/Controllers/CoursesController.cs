using System.Net;
using System.Threading.Tasks;
using FooWebApp.DataContracts;
using FooWebApp.Services;
using FooWebApp.Store;
using Microsoft.AspNetCore.Mvc;

namespace FooWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public CoursesController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet("{courseName}/students/{studentId}")]
        public async Task<IActionResult> GetStudent(string courseName, string studentId)
        {
            var student = await _studentService.GetStudent(courseName, studentId);
            return Ok(student);
        }

        [HttpPost("{courseName}/students")]
        public async Task<IActionResult> AddStudent(string courseName, [FromBody] Student student)
        {
            await _studentService.AddStudent(courseName, student);
            return CreatedAtAction(nameof(GetStudent), new { courseName = courseName, studentId = student.Id }, student);
        }

        [HttpDelete("{courseName}/students/{studentId}")]
        public async Task<IActionResult> DeleteStudent(string courseName, string studentId)
        {
            await _studentService.DeleteStudent(courseName, studentId);
            return Ok();
        }

        [HttpGet("{courseName}/students")]
        public async Task<IActionResult> GetStudents(string courseName, string continuationToken, int limit)
        {
            GetStudentsResult result = await _studentService.GetStudents(courseName, continuationToken, limit);

            string nextUri = null;
            if (!string.IsNullOrWhiteSpace(result.ContinuationToken))
            {
                nextUri = $"api/courses/{courseName}/students?continuationToken={WebUtility.UrlEncode(result.ContinuationToken)}&limit={limit}";
            }

            var response = new GetStudentsResponse
            {
                NextUri = nextUri,
                Students = result.Students
            };
            return Ok(response);
        }

        [HttpPut("{courseName}/students/{studentId}")]
        public async Task<IActionResult> UpdateStudent(string courseName, string studentId, [FromBody] UpdateStudentRequestBody body)
        {
            var student = new Student
            {
                Id = studentId,
                Name = body.Name,
                GradePercentage = body.GradePercentage
            };

            await _studentService.UpdateStudent(courseName, student);
            return Ok();
        }

        private bool ValidateStudent(Student student, out string error)
        {
            if (string.IsNullOrWhiteSpace(student.Id))
            {
                error = "The id must not be empty";
                return false;
            }
            if (string.IsNullOrWhiteSpace(student.Name))
            {
                error = "The Name must not be empty";
                return false;
            }
            if (student.GradePercentage < 0 || student.GradePercentage > 100)
            {
                error = $"GradePercentage {student.GradePercentage} is not valid";
                return false;
            }
            error = "";
            return true;
        }
    }
}
