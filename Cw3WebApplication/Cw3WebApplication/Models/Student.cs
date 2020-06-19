using System;

namespace Cw3WebApplication.Models
{
    public class Student
    {
        public string IndexNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public int IdEnrollment { get; set; }
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
        public string Refreshtoken { get; set; }

        public virtual Enrollment IdEnrollmentNavigation { get; set; }
        //public string Studies { get; internal set; }
        //public short Semester { get; internal set; }
    }
}
