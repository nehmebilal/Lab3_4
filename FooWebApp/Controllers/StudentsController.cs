using System;
using System.Threading.Tasks;
using FooWebApp.DataContracts;
using FooWebApp.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FooWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentStore _studentStore;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentStore studentStore, ILogger<StudentsController> logger)
        {
            _studentStore = studentStore;
            _logger = logger;
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
            catch (StudentNotFoundException)
            {
                return NotFound($"The student with id {id} was not found");
            }
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to retrieve student {id} from storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while retrieving student {id} from storage");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
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
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed add student {student} to storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while adding student {student} to storage");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
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
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to delete student {id} from storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while deleting student {id} from storage");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            try
            {
                var students = await _studentStore.GetStudents();
                var response = new GetStudentsResponse
                {
                    Students = students
                };
                return Ok(response);
            }
            catch (StorageErrorException)
            {
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] UpdateStudentRequestBody updateStudentRequestBody)
        {
            try
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
            catch (StorageErrorException e)
            {
                _logger.LogError(e, $"Failed to update student {id} in storage");
                return StatusCode(503, "The service is unavailable, please retry in few minutes");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unknown exception occured while updating student {id} in storage");
                return StatusCode(500, "An internal server error occured, please reachout to support if this error persists");
            }
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
