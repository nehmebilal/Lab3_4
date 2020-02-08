﻿using System.Collections.Generic;
using FooWebApp.DataContracts;
using System.Threading.Tasks;

namespace FooWebApp.Store
{
    public interface IStudentStore
    {
        Task AddStudent(Student student);
        Task<Student> GetStudent(string id);
        Task DeleteStudent(string studentId);
        Task<List<Student>> GetStudents();
        Task UpdateStudent(string studentId,Student student);
    }
}
