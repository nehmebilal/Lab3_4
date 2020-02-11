using System.Threading.Tasks;
using FooWebApp.DataContracts;
using FooWebApp.Store;
using Microsoft.AspNetCore.Mvc;

namespace FooWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentStore _studentStore;

        public StudentsController(IStudentStore studentStore)
        {
            _studentStore = studentStore;
        }

        // GET api/students/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("The id must not be empty");
            }

            try
            {
                Student student = await _studentStore.GetStudent(id);
                return Ok(student);
            }
            catch (StudentNotFoundException)
            {
                return NotFound($"The student with id {id} was not found");
            }
        }

        // POST api/students
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student student)
        {
            if (!ValidateStudent(student, out string error))
            {
                return BadRequest(error);
            }

            try
            {
                await _studentStore.AddStudent(student);
                return Ok();
            }
            catch (StudentAlreadyExistsException)
            {
                return Conflict($"Student {student.Id} already exists");
            }
        }

        // DELETE api/students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("The id must not be empty");
            }
            try
            {
                await _studentStore.DeleteStudent(id);
                return Ok();
            }
            catch (StudentNotFoundException)
            {
                return NotFound($"The student with id {id} was not found");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _studentStore.GetStudents();
            var response = new GetStudentsResponse
            {
                Students = students
            };
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] UpdateStudentRequestBody updateStudentRequestBody)
        {
            var student = new Student
            {
                Id = id,
                Name = updateStudentRequestBody.Name,
                GradePercentage = updateStudentRequestBody.GradePercentage
            };

            if (!ValidateStudent(student, out string error))
            {
                return BadRequest(error);
            }

            await _studentStore.UpdateStudent(student);
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
