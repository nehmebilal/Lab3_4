using FooWebApp.DataContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FooWebApp.Store
{
    public class InMemoryStudentStore : IStudentStore
    {
        private readonly Dictionary<string, Student> _students = new Dictionary<string, Student>();

        public Task AddStudent(Student student)
        {
            if (!_students.TryAdd(student.Id, student))
            {
                throw new StudentAlreadyExistsException($"Student {student.Id} already exists");
            }

            // we return Task.CompletedTask because this implementation is synchronous (i.e. it doesn't await any async operations)
            return Task.CompletedTask;
        }

        public Task<Student> GetStudent(string id)
        {
            if (!_students.TryGetValue(id, out var student))
            {
                throw new StudentNotFoundException($"Student {id} was not found");
            }
            return Task.FromResult(student);
        }

        public Task DeleteStudent(string studentId)
        {
            bool deleted = _students.Remove(studentId);
            if (!deleted)
            {
                throw new StudentNotFoundException($"Student {studentId} was not found");
            }
            return Task.FromResult(deleted);
        }
    }
}
