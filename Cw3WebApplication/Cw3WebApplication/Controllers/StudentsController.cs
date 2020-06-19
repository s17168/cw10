using System;
using Cw3WebApplication.Models;
using Cw3WebApplication.DAL;
using Microsoft.AspNetCore.Mvc;
using Cw3WebApplication.DTOs.Requests;

namespace Cw3WebApplication.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public StudentsController(IDbService service)
        {
            _dbService = service;
        }

        [HttpGet("{id}")]
        public IActionResult GetStudent(string id)
        {
            if (id == "1")
            {
                return Ok("Kowalski");
            }

            var student = _dbService.GetStudent(id);

            return Ok(student);
        }

        [HttpGet]
        public IActionResult GetStudents([FromQuery] string orderBy)
        {
            return Ok(_dbService.GetAllStudents());
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 10000)}";
            _dbService.AddStudent(student);
            return Ok(student);
        }


        [HttpPut("{id}")]
        public IActionResult UpdateStudent(StudentDto studentDto, string id)
        {
            var studentId = studentDto.IndexNumber; // should check if id is actually correct index number
            _dbService.UpdateStudent(studentDto, id);
            return Ok("Akutalizacja dokonczona dla studenta " + studentId);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(string id)
        {
            _dbService.DeleteStudent(id);
            return Ok("Usuwanie ukonczone dla studenta id = " + id);
        }

        [HttpGet]
        [Route("{idStudent}/enrollment")]
        public IActionResult GetEnrollment(string idStudent)
        {
            return Ok(_dbService.GetEnrollment(idStudent));
        }

    }

}