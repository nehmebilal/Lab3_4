using FooWebApp.DataContracts;
using System.Threading.Tasks;

namespace FooWebApp.Store
{
    public interface IStudentStore
    {
        Task AddStudent(Student student);
        Task<Student> GetStudent(string id);
        Task DeleteStudent(string studentId);
    }
}
