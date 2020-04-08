﻿using Cw3WebApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3WebApplication.DAL
{
    public class MsqlDbService : IDbService
    {

        private static List<Student> _students;

        static MsqlDbService()
        {
            _students = new List<Student>();
        }

        public void AddStudent(Student student)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Student> GetStudents()
        {
            _students = new List<Student>(); //clean already added students, alwes retrieve new List from DB

            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s17168;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "select Student.FirstName, Student.LastName, Student.IndexNumber, Student.BirthDate, Student.IdEnrollment, Studies.Name, Enrollment.Semester "
                    + "from Student inner join Enrollment on Student.IdEnrollment = Enrollment.IdEnrollment inner join studies "
                    + "on Studies.IdStudy = Enrollment.IdStudy";

                connection.Open();

                var dr = command.ExecuteReader();
                while (dr.Read())
                {
                    Console.WriteLine(dr);
                    var student = new Student();
                    student.FirstName = dr["FirstName"].ToString();
                    student.LastName = dr["LastName"].ToString();
                    student.IndexNumber = dr["IndexNumber"].ToString();
                    student.BirthDate = dr["BirthDate"].ToString();
                    student.IdEnrollment = Int16.Parse(dr["IdEnrollment"].ToString());

                    student.StudyName = dr["Name"].ToString();
                    student.Semester = Int16.Parse(dr["Semester"].ToString());

                    _students.Add(student);
                }
            }
            return _students;
        }

        public Enrollment GetEnrollment(string idStudent)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s17168;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                Console.WriteLine(idStudent);
                command.Connection = connection;
                command.CommandText = "select IndexNumber, Enrollment.IdEnrollment, Enrollment.Semester, Enrollment.IdStudy, " 
                    + "Enrollment.StartDate from Student inner join Enrollment on Enrollment.idEnrollment = student.idEnrollment "
                    + "where Student.IndexNumber = @id" + ";";

                command.Parameters.AddWithValue("@id", idStudent);  // this prevents below sql injection
               
                // SQL injetion example: hit following URL to drop table Student
                // localhost:51290/api/students/23';%20drop%20table%20student;%20select%20*%20from%20student%20where%20FirstName%20=%20'/enrollment
                
                connection.Open();
                
                var dr = command.ExecuteReader();
                while (dr.Read())
                {
                    Console.WriteLine(dr);
                    var enrolment = new Enrollment
                    {
                        IdEnrollment = Int16.Parse(dr["IdEnrollment"].ToString()),
                        Semester = Int16.Parse(dr["Semester"].ToString()),
                        IdStudy = Int16.Parse(dr["IdStudy"].ToString()),
                        StartDate = dr["StartDate"].ToString()
                    };
                    return enrolment;
                }
            }
            throw new Exception("Error fetching query from DB or student with id " + idStudent + " doesn't exist or is not enrolled"); // should not happen
        }

    }
}
