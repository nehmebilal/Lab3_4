using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FooWebApp.DataContracts;
using FooWebApp.Store;
using Microsoft.ApplicationInsights;
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
        private readonly TelemetryClient _telemetryClient;

        public StudentsController(IStudentStore studentStore, ILogger<StudentsController> logger, TelemetryClient telemetryClient)
        {
            _studentStore = studentStore;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        // GET api/students/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var stopWatch = Stopwatch.StartNew();
                Student student = await _studentStore.GetStudent(id);
                _telemetryClient.TrackMetric("StudentStore.GetStudent.Time", stopWatch.ElapsedMilliseconds);
                return Ok(student);
            }
            catch (StudentNotFoundException)
            {
                _logger.LogWarning($"Student {id} was not found");
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
                var stopWatch = Stopwatch.StartNew();
                await _studentStore.AddStudent(student);
                _telemetryClient.TrackMetric("StudentStore.AddStudent.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("StudentAdded");

                return CreatedAtAction(nameof(Get), new { id = student.Id}, student);
            }
            catch (StudentAlreadyExistsException)
            {
                _logger.LogWarning($"Student {student.Id} already exists");
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
                var stopWatch = Stopwatch.StartNew();
                await _studentStore.DeleteStudent(id);
                _telemetryClient.TrackMetric("StudentStore.DeleteStudent.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("StudentDeleted");

                return Ok();
            }
            catch (StudentNotFoundException)
            {
                _logger.LogWarning($"Student {id} was not found");
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
                var stopWatch = Stopwatch.StartNew();
                var students = await _studentStore.GetStudents();
                _telemetryClient.TrackMetric("StudentStore.GetStudents.Time", stopWatch.ElapsedMilliseconds);
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
                    _logger.LogWarning(error);
                    return BadRequest(error);
                }

                var stopWatch = Stopwatch.StartNew();
                await _studentStore.UpdateStudent(student);
                _telemetryClient.TrackMetric("StudentStore.UpdateStudents.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("StudentUpdated");

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
