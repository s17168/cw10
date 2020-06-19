using Cw3WebApplication.DTOs.Requests;
using Cw3WebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3WebApplication.DAL
{
    public interface IDbService
    {
        IEnumerable<Student> GetStudents();
        void AddStudent(Student student);

        Enrollment GetEnrollment(string idStudent);
        Student GetStudent(string id);
        IEnumerable<Student> GetAllStudents();
        Student UpdateStudent(StudentDto studentDto, string id);

        void DeleteStudent(string indexNumber);
    }
}
