using System.Collections.Concurrent;
using FooWebApp.DataContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FooWebApp.Store
{
    public class InMemoryStudentStore : IStudentStore
    {
        private readonly ConcurrentDictionary<string, Student> _students = new ConcurrentDictionary<string, Student>();

        public Task AddStudent(Student student)
        {
            CheckStudentProperties(student);
            if (!_students.TryAdd(student.Id, student))
            {
                throw new StudentAlreadyExistsException($"Student {student.Id} already exists");
            }

            // we return Task.CompletedTask because this implementation is synchronous (i.e. it doesn't await any async operations)
            return Task.CompletedTask;
        }

        public Task<Student> GetStudent(string id)
        {
            Utilities.CheckNullOrWhitespace(nameof(id),id);
            if (!_students.TryGetValue(id, out var student))
            {
                throw new StudentNotFoundException($"Student {id} was not found");
            }
            return Task.FromResult(student);
        }

        public Task DeleteStudent(string studentId)
        {
            Utilities.CheckNullOrWhitespace(nameof(studentId),studentId);
            bool deleted = _students.TryRemove(studentId,out var student);
            if (!deleted)
            {
                throw new StudentNotFoundException($"Student {studentId} was not found");
            }
            return Task.FromResult(deleted);
        }

        public Task<List<Student>> GetStudents()
        {
            var students = new List<Student>(_students.Values);
            return Task.FromResult(students);
        }

        public async Task UpdateStudent(string studentId,Student student)
        {
            Utilities.CheckNullOrWhitespace(nameof(studentId),studentId);
            Utilities.CheckNullOrWhitespace(nameof(student.Name),student.Name);
            
            var retrieved = !_students.TryGetValue(studentId, out var existingStudent);
            if (!retrieved)
            {
                await AddStudent(student);
            }
            else
            {
                _students.TryUpdate(studentId, student, existingStudent);
            }
        }

        private static void CheckStudentProperties(Student student)
        {
            Utilities.CheckNullOrWhitespace(nameof(student.Id),student.Id);
            Utilities.CheckNullOrWhitespace(nameof(student.Name),student.Name);
        }
    }
}
