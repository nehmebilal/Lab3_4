﻿using FooWebApp.DataContracts;
using System.Threading.Tasks;

namespace FooWebApp.Store
{
    public interface IStudentStore
    {
        Task AddStudent(string courseName, Student student);
        Task<Student> GetStudent(string courseName, string id);
        Task DeleteStudent(string courseName, string studentId);
        Task<GetStudentsResult> GetStudents(string courseName, string continuationToken, int limit);
        Task UpdateStudent(string courseName, Student student);
    }
}
