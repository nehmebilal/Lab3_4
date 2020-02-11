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
            if (!_students.TryAdd(student.Id, student))
            {
                throw new StudentAlreadyExistsException($"Student {student.Id} already exists");
            }
            return Task.CompletedTask;
        }

        public Task<Student> GetStudent(string id)
        {
            if (!_students.TryGetValue(id, out Student student))
            {
                throw new StudentNotFoundException($"Student {id} was not found");
            }
            return Task.FromResult(student);
        }

        public Task DeleteStudent(string studentId)
        {
            bool deleted = _students.TryRemove(studentId, out Student student);
            if (!deleted)
            {
                throw new StudentNotFoundException($"Student {studentId} was not found");
            }
            return Task.CompletedTask;
        }

        public Task<List<Student>> GetStudents()
        {
            var students = new List<Student>(_students.Values);
            students.Sort((first, second) => second.GradePercentage.CompareTo(first.GradePercentage));
            return Task.FromResult(students);
        }

        public Task UpdateStudent(Student student)
        {
            _students[student.Id] = student;
            return Task.CompletedTask;
        }
    }
}
