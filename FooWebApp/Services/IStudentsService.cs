using FooWebApp.DataContracts;
using FooWebApp.Store;
using System.Threading.Tasks;

namespace FooWebApp.Services
{
    public interface IStudentService
    {
        Task AddStudent(string courseName, Student student);
        Task DeleteStudent(string courseName, string studentId);
        Task<Student> GetStudent(string courseName, string studentId);
        Task<GetStudentsResult> GetStudents(string courseName, string continuationToken, int limit);
        Task UpdateStudent(string courseName, Student student);
    }
}