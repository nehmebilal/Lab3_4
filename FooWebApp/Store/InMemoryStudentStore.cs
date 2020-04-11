using FooWebApp.DataContracts;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace FooWebApp.Store
{
    public class InMemoryStudentStore : IStudentStore
    {
        private ConcurrentDictionary<string, Student> _students = new ConcurrentDictionary<string, Student>();

        public Task AddStudent(string courseName, Student student)
        {
            string key = GetKey(courseName, student.Id);
            if (!_students.TryAdd(key, student))
            {
                throw new StorageErrorException($"Student {student.Id} already exists in course {courseName}", 409);
            }
            return Task.CompletedTask;
        }

        private static string GetKey(string courseName, string studentId)
        {
            return $"{courseName}_{studentId}";
        }

        public Task DeleteStudent(string courseName, string studentId)
        {
            string key = GetKey(courseName, studentId);
            if (!_students.TryRemove(key, out _))
            {
                throw new StorageErrorException($"Student {studentId} was not found in course {courseName}", 404);
            }
            return Task.CompletedTask;
        }

        public Task<Student> GetStudent(string courseName, string studentId)
        {
            string key = GetKey(courseName, studentId);
            if (!_students.TryGetValue(key, out Student student))
            {
                throw new StorageErrorException($"Student {studentId} was not found in course {courseName}", 404);
            }
            return Task.FromResult(student);
        }

        public Task<GetStudentsResult> GetStudents(string courseName, string continuationToken, int limit)
        {
            int offset = 0;
            if (!string.IsNullOrEmpty(continuationToken))
            {
                // we are storing the offset in the continuation token
                // remember that we can store whatever we want in the continuation token as long as it can be used
                // to fetch the next page
                offset = int.Parse(continuationToken); 
            }

            var keys = _students.Keys.Where(key => key.StartsWith($"{courseName}_"));
            var students = keys.Select(key => _students[key]).ToList();
            students.Sort((first, second) => second.GradePercentage.CompareTo(first.GradePercentage));
            students = students.Skip(offset).ToList(); // skip previous pages;

            if (students.Count <= limit) // if this is the last page
            {
                return Task.FromResult(
                    new GetStudentsResult
                {
                    ContinuationToken = null, // this is the last page
                    Students = students
                });
            }

            // if we get here, it means there are more pages available
            return Task.FromResult(
                new GetStudentsResult
            {
                ContinuationToken = (offset + limit).ToString(),
                Students = students.Take(limit).ToList()
            });
        }

        public Task UpdateStudent(string courseName, Student student)
        {
            string key = GetKey(courseName, student.Id);
            _students[key] = student;
            return Task.CompletedTask;
        }
    }
}
