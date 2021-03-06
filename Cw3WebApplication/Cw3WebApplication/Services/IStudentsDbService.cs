﻿using Cw3WebApplication.DTOs.Requests;
using Cw3WebApplication.DTOs.Responses;
using Cw3WebApplication.Models;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace Wyklad5.Services
{
    public interface IStudentsDbService
    {
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);
        void PromoteStudents(int semester, string studies);
        Student GetStudent(string index);
        JwtSecurityToken GetJwtToken();
        void RegisterNewStudent(Student student, string password);
        bool CheckUserPassword(Student student, string password);
        void SaveRefreshTokenInDb(Guid refreshToken, Student student);
    }
}
