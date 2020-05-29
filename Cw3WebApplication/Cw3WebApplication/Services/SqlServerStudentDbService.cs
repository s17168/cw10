﻿using Cw3WebApplication.DTOs.Requests;
using Cw3WebApplication.DTOs.Responses;
using Cw3WebApplication.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Wyklad5.Services
{
    public class SqlServerStudentDbService : IStudentsDbService
    {
        //public object Configuration { get; private set; }

        public IConfiguration Configuration { get; set; }

        private static string sqlConnecionStr = "Data Source=db-mssql;Initial Catalog=s17168;Integrated Security=True";

        public SqlServerStudentDbService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            var idStudy = -1;
            var nextIdEnrollment = -1;

            var startDate = DateTime.Now;
            var semester = 1;

            using (var connection = new SqlConnection(sqlConnecionStr))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();
                var transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                try
                {
                    //1. Check wether studies with given name exist
                    command.CommandText = "SELECT IdStudy from Studies where Name = @name" + ";";
                    command.Parameters.AddWithValue("@name", request.Studies);
                    var dr = command.ExecuteReader();
                    if (!dr.Read())
                    {
                        dr.Close();
                        transaction.Rollback();
                        throw new Exception("Studia o podanej nazwie nie istnieją!");
                    }
                    idStudy = (int)dr["IdStudy"];
                    dr.Close();

                    //2. Check wether indexNumber is unique (it's a PK in Student)
                    command.CommandText = "SELECT IndexNumber from Student where IndexNumber = @number" + ";";
                    command.Parameters.AddWithValue("@number", request.IndexNumber);
                    var dr2 = command.ExecuteReader();
                    if (dr2.Read())
                    {
                        dr2.Close();
                        transaction.Rollback();
                        throw new Exception("Student o podanym nr indeksu juz istnieje!");
                    }
                    dr2.Close();


                    //3. Check latest IdEnrollment
                    command.CommandText = "SELECT Max(IdEnrollment) as max from Enrollment;";
                    var dr3 = command.ExecuteReader();
                    if (!dr3.Read())
                    {
                        dr3.Close();
                        transaction.Rollback();
                        throw new Exception("Problem with fetching id enrollemnt");
                    }
                    var maxIdEnrollment = (int)dr3["max"];
                    nextIdEnrollment = ++maxIdEnrollment;
                    dr3.Close();


                    //4. Create new Enrollment for student
                    command.CommandText = "INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate) "
                        + "VALUES(@nextId, @semester, @idStudy, CAST(@date AS DATETIME));";
                    command.Parameters.AddWithValue("@nextId", nextIdEnrollment);
                    command.Parameters.AddWithValue("@semester", semester);
                    command.Parameters.AddWithValue("@idStudy", idStudy);
                    command.Parameters.AddWithValue("@date", startDate);

                    command.ExecuteNonQuery();

                    //5. Add student to Studyies
                    command.CommandText = "INSERT INTO Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) "
                        + "VALUES(@indexNumber, @studentFirstName, @studnetLastName, CAST(@bdate AS DATE), @idEnrollment)";
                    command.Parameters.AddWithValue("@indexNumber", request.IndexNumber);
                    command.Parameters.AddWithValue("@studentFirstName", request.FirstName);
                    command.Parameters.AddWithValue("@studnetLastName", request.LastName);
                    command.Parameters.AddWithValue("@bdate", request.BirthDate);
                    command.Parameters.AddWithValue("@idEnrollment", nextIdEnrollment);

                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (SqlException exc)
                {
                    transaction.Rollback();
                    Console.WriteLine(exc.Message);
                    throw new Exception("SqlException " + exc.Message);
                }
            }

            var response = new EnrollStudentResponse();
            response.IdEnrollment = nextIdEnrollment;
            response.Semester = semester;
            response.IdStudy = idStudy;
            response.StartDate = startDate;

            return response;
        }

        public void PromoteStudents(int semester, string studies) // params not in body but in query string! 
        {
            using (var connection = new SqlConnection(sqlConnecionStr))
            using (var command = new SqlCommand("PromoteStudents"))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Connection = connection;

                command.Parameters.AddWithValue("@studyName", studies);
                command.Parameters.AddWithValue("@existingSemester", semester);

                connection.Open();
                command.ExecuteNonQuery();
            }   
        }

        public Student GetStudent(string index)
        {
            var student = new Student();

            using (var connection = new SqlConnection(sqlConnecionStr))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;

                //command.CommandText= "SELECT * FROM Student;";
                command.CommandText="SELECT * FROM Student WHERE Student.IndexNumber = @index;";
                command.Parameters.AddWithValue("@index", index);

                connection.Open();

                var dr = command.ExecuteReader();

                if (!dr.Read())
                {
                    return null;
                }
                student.FirstName = dr["FirstName"].ToString();
                student.LastName = dr["LastName"].ToString();
                student.IndexNumber = dr["IndexNumber"].ToString();
                student.HashedPassword = dr["HashedPassword"].ToString();
                student.Salt = dr["Salt"].ToString();
                student.Refreshtoken = dr["Refreshtoken"].ToString();
            }
            return student;
        }

        public JwtSecurityToken GetJwtToken()
        {

            var claims = new[]
{
                new Claim(ClaimTypes.NameIdentifier, "1"),
                //new Claim(ClaimTypes.Name, "jan123"),
                //new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds
            );

            return token;
        }

        public void RegisterNewStudent(Student student, string plainTextPassword) 
        {
            // This function is to register new student 
            // we will created hashed salted password and store it in db with salt
            //Console.WriteLine("Registering new student with password: " + plainTextPassword); // only for debug~!
            var salt = CreateSalt();
            var saltedPassword = Create(plainTextPassword, salt);

            student.HashedPassword = saltedPassword;
            student.Salt = salt;

            //Console.WriteLine("To register in DB hashed pass " + saltedPassword + ", salt " + salt); // only for debug~!

            using (var connection = new SqlConnection(sqlConnecionStr))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;

                command.CommandText = "UPDATE student set HashedPassword = @hashedpass, Salt = @salt WHERE Student.IndexNumber = @index;";
                command.Parameters.AddWithValue("@index", student.IndexNumber);
                command.Parameters.AddWithValue("@hashedpass", student.HashedPassword);
                command.Parameters.AddWithValue("@salt", student.Salt);

                connection.Open();
                command.ExecuteNonQuery();
            }

            //Console.WriteLine("Registered in DB hashed pass " + saltedPassword + ", salt " + salt); // only for debug~!
        }

        public bool CheckUserPassword(Student student, string plainTextUserPass)
        {
            var salt = student.Salt;
            var hashedPass = student.HashedPassword;

            if (Validate(plainTextUserPass, salt, hashedPass))
            {
                Console.WriteLine("Password validated");
                return true;
            }
            else {
                Console.WriteLine("Could not validate user password.");
                return false;
            }
        }

        public static string Create(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                    password: value,
                    salt: Encoding.UTF8.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 10000,
                    numBytesRequested: 128);
            return Convert.ToBase64String(valueBytes);
        }

        // checking if password pass which is in DB matches the new pass hash which
        // is generated with current hashed pass with current salt
        public static bool Validate(string userPass, string salt, string hash)
            => Create(userPass, salt) == hash;

        public static string CreateSalt()
        {
            byte[] randomBytes = new byte[64]; // or: 128 / 
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

    }
}
