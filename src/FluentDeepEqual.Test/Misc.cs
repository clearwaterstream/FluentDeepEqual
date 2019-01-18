using System;
using System.Collections.Generic;
using System.Text;

namespace FluentDeepEqual.Test
{
    class Student
    {
        public string Name { get; set; }
        public int? GPA { get; set; }
    }

    class Class
    {
        public IEnumerable<Student> AttendingStudents { get; set; }
        public string Subject { get; set; }
    }
}
