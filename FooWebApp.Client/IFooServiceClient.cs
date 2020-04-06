using FooWebApp.DataContracts;
using System.Threading.Tasks;

namespace FooWebApp.Client
{
    public interface IFooServiceClient
    {
        Task AddStudent(string courseName, Student student);
        Task<Student> GetStudent(string courseName, string id);
        Task UpdateStudent(string courseName, Student student);
        Task DeleteStudent(string courseName, string id);
        Task<GetStudentsResponse> GetStudents(string courseName, int limit);
        Task<GetStudentsResponse> GetStudentsByUri(string uri);
    }
}