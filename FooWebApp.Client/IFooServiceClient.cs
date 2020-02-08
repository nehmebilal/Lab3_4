using FooWebApp.DataContracts;
using System.Threading.Tasks;

namespace FooWebApp.Client
{
    public interface IFooServiceClient
    {
        Task AddStudent(Student student);
        Task<Student> GetStudent(string id);
    }
}