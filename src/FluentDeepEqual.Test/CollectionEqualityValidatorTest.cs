using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FluentDeepEqual.Test
{
    public class CollectionEqualityValidatorTest
    {
        [Fact]
        public void Same()
        {
            var c1 = new Class()
            {
                Subject = "Math",
                AttendingStudents = new List<Student>
                {
                    new Student()
                    {
                        Name = "Joe"
                    },
                    new Student
                    {
                        Name = "Joanne",
                        GPA = 61
                    }
                }
            };

            var c2 = new Class()
            {
                Subject = "Math",
                AttendingStudents = new List<Student>
                {
                    new Student()
                    {
                        Name = "Joe"
                    },
                    new Student
                    {
                        Name = "Joanne",
                        GPA = 61
                    }
                }
            };

            var comparer = new ClassComparer();

            var r = comparer.Compare(c1, c2);
        }

        [Fact]
        public void CollCountMistmatch()
        {
            var c1 = new Class()
            {
                Subject = "Math",
                AttendingStudents = new List<Student>
                {
                    new Student()
                    {
                        Name = "Joe"
                    },
                    new Student
                    {
                        Name = "Joanne",
                        GPA = 61
                    }
                }
            };

            var c2 = new Class()
            {
                Subject = "Math",
                AttendingStudents = new List<Student>
                {
                    new Student()
                    {
                        Name = "Joe"
                    }
                }
            };

            var comparer = new ClassComparer();

            var r = comparer.Compare(c1, c2);
        }

        [Fact]
        public void DiffStudents()
        {
            var c1 = new Class()
            {
                Subject = "Math",
                AttendingStudents = new List<Student>
                {
                    new Student()
                    {
                        Name = "Joe"
                    },
                    new Student
                    {
                        Name = "Joanne",
                        GPA = 61
                    }
                }
            };

            var c2 = new Class()
            {
                Subject = "Math",
                AttendingStudents = new List<Student>
                {
                    new Student()
                    {
                        Name = "Alex"
                    },
                    new Student
                    {
                        Name = "Suzanne",
                        GPA = 61
                    }
                }
            };

            var comparer = new ClassComparer();

            var r = comparer.Compare(c1, c2);
        }

        [Fact]
        public void DiffValuesInStudents()
        {
            var c1 = new Class()
            {
                Subject = "Math",
                AttendingStudents = new List<Student>
                {
                    new Student()
                    {
                        Name = "Joe"
                    },
                    new Student
                    {
                        Name = "Joanne",
                        GPA = 61
                    }
                }
            };

            var c2 = new Class()
            {
                Subject = "Math",
                AttendingStudents = new List<Student>
                {
                    new Student()
                    {
                        Name = "Joe"
                    },
                    new Student
                    {
                        Name = "Joanne",
                        GPA = 62
                    }
                }
            };

            var comparer = new ClassComparer();

            var r = comparer.Compare(c1, c2);
        }
    }

    class ClassComparer : EqualityValidator<Class>
    {
        public ClassComparer()
        {
            RuleFor(x => x.Subject).Same();
            RuleForCollection(x => x.AttendingStudents, s => s.Name).SetValidator(new StudentComparator());
        }
    }
}
