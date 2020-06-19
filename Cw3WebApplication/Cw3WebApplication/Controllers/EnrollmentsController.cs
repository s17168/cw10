using System;
using Cw3WebApplication.DTOs.Requests;
using Cw3WebApplication.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wyklad5.Services;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Cw3WebApplication.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService _studentsDbService;
        public IConfiguration Configuration { get; set; }


        public EnrollmentsController(IStudentsDbService service, IConfiguration configuration)
        {
            _studentsDbService = service;
            Configuration = configuration;
        }

        //adding new student and enrolls him to semester
        [HttpPost]
        //[Authorize(Roles = "employee")] // removed for quick debugging
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            var response = new EnrollStudentResponse();
            try
            {
                response = _studentsDbService.EnrollStudent(request);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
            return Ok(response);
        }

        //promotes all students of given study name to given semester
        [HttpPost]
        [Route("promotions")]
       // [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(int semester, string studies) // params not in body but in query string! 
        {
            try
            {
                _studentsDbService.PromoteStudents(semester, studies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Successfully promoted all students to next semester");
        }

        // Student uses it to provide his password - temporary we are letting anyone create his password
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public IActionResult RegisterNewUser(LoginRequestDto request)
        {
            Console.WriteLine("Register new user");
            var student = _studentsDbService.GetStudent(request.Login);
            if (student.HashedPassword.Equals(""))
            {
                // We are good to go, we can register new student - he haven't registered before
                Console.WriteLine("New user will be registered. Saving his credentials to DB");
            } else
            {
                return BadRequest("Student is already registered. ");
            }

            //saving student's password in DB (hashed + salt)
            _studentsDbService.RegisterNewStudent(student, request.Password);
            return Ok("Your password has been saved");
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public IActionResult Login(LoginRequestDto request)
        {
            // TODO: Move all this logic to DBService
            var student = _studentsDbService.GetStudent(request.Login);

            if (_studentsDbService.CheckUserPassword(student, request.Password))
            {
                Console.WriteLine("Password confirmed");
            }
            else
            {
                return BadRequest("Password dont match");
            }

            var token = _studentsDbService.GetJwtToken();
            var refreshToken = Guid.NewGuid(); // is unique and has no information 

            _studentsDbService.SaveRefreshTokenInDb(refreshToken, student);

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });
        }

        [HttpPost("refresh-token/{reftoken}")]
        public IActionResult RefreshToken(string reftoken, string login)
        {
            Console.WriteLine("Refresh token from client: "+ reftoken);
            var student = _studentsDbService.GetStudent(login);

            // spr w bazie czy istnieje juz token - jak tak to zwracamy nowy
            bool tokenExists = student.Refreshtoken == reftoken; 
            if (!tokenExists)
            {
                return BadRequest();
            }

            var token = _studentsDbService.GetJwtToken();
            var refreshToken = Guid.NewGuid(); // is unique and has no information 

            _studentsDbService.SaveRefreshTokenInDb(refreshToken, student);

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });
        }

    }
}
