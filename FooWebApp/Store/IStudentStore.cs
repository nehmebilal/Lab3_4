using System.Collections.Generic;
using FooWebApp.DataContracts;
using System.Threading.Tasks;

namespace FooWebApp.Store
{
    public interface IStudentStore
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        /// <exception cref="StorageErrorException"
        Task AddStudent(Student student);
        Task<Student> GetStudent(string id);
        Task DeleteStudent(string studentId);
        Task<List<Student>> GetStudents();
        Task UpdateStudent(Student student);
    }
}
