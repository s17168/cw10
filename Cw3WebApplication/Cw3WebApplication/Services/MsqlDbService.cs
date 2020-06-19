using Cw3WebApplication.DTOs.Requests;
using Cw3WebApplication.Models;
using Cw3WebApplication.NewFolder;
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

        private static string sqlConnecionStr = "Data Source=DESKTOP-H0B9S2Q\\SQLEXPRESS;Initial Catalog=apbd;Integrated Security=True";

        private readonly apbdContext _dbcontext;
        public MsqlDbService(apbdContext context)
        {
            _students = new List<Student>();
            _dbcontext = context;
        }

        public void AddStudent(Student student)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Student> GetStudents()
        {
            _students = new List<Student>(); //clean already added students, alwes retrieve new List from DB

            using (var connection = new SqlConnection(sqlConnecionStr))
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
                    student.BirthDate = Convert.ToDateTime(dr["BirthDate"].ToString());
                    student.IdEnrollment = Int16.Parse(dr["IdEnrollment"].ToString());

                    //student.Studies = dr["Name"].ToString();
                    //student.Semester = Int16.Parse(dr["Semester"].ToString());

                    _students.Add(student);
                }
            }
            return _students;
        }

        public Enrollment GetEnrollment(string idStudent)
        {
            using (var connection = new SqlConnection(sqlConnecionStr))
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
                        StartDate = Convert.ToDateTime(dr["StartDate"].ToString())
                };
                    return enrolment;
                }
            }
            throw new Exception("Error fetching query from DB or student with id " + idStudent + " doesn't exist or is not enrolled"); // should not happen
        }

        public Student GetStudent(string indexNumber)
        {
            var student = (Student) _dbcontext.Student.Where(d => d.IndexNumber.Equals(indexNumber)).FirstOrDefault<Student>();
            return student;
        }

        public IEnumerable<Student> GetAllStudents()
        {
            var students = _dbcontext.Student.ToList();
            return students;
        }

        public Student UpdateStudent(StudentDto studentDto, string id)
        {
            var existingStudent = (Student)_dbcontext.Student.FirstOrDefault<Student>();

            if (studentDto.FirstName != null && studentDto.FirstName.Equals(""))
            {
                existingStudent.FirstName = studentDto.FirstName;
            }
            if (studentDto.LastName != null && studentDto.LastName.Equals(""))
            {
                existingStudent.LastName = studentDto.LastName;
            }
            if (studentDto.BirthDate != null && studentDto.BirthDate.Equals(""))
            {
                existingStudent.BirthDate = studentDto.BirthDate;
            }
            if (studentDto.IdEnrollment.Equals(""))
            {
                existingStudent.IdEnrollment = studentDto.IdEnrollment;
            }
            _dbcontext.SaveChanges();
            return existingStudent;
        }

        public void DeleteStudent(string indexNumber)
        {
            var existingStudent = (Student)_dbcontext.Student.FirstOrDefault<Student>();
            _dbcontext.Remove(existingStudent);
        }
    }
}
