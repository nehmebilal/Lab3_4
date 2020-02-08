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
            try
            {
                Student student = await _studentStore.GetStudent(id);
                return Ok(student);
            }
            catch(StudentNotFoundException)
            {
                return NotFound($"The student with id {id} was not found");
            }
        }

        // POST api/students
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student student)
        {
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
    }
}
