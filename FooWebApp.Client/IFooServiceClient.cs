using System.Collections.Generic;
using FooWebApp.DataContracts;
using System.Threading.Tasks;

namespace FooWebApp.Client
{
    public interface IFooServiceClient
    {
        Task AddStudent(Student student);
        Task<Student> GetStudent(string id);
        Task UpdateStudent(Student student);
        Task<GetStudentsResponse> GetStudents();
        Task DeleteStudent(string id);
    }
}