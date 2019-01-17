﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace FluentDeepEqual.Test
{
    public class EqualityValidatorTest
    {
        [Fact]
        public void Basic()
        {
            var v = new StudentComparator();

            var s1 = new Student()
            {
                Name = "Frank",
                Score = 67
            };

            var s2 = new Student()
            {
                Name = "Frank",
                Score = 84
            };


            var result = v.Compare(s1, s2);
        }
    }

    class StudentComparator : EqualityValidator<Student>
    {
        public StudentComparator()
        {
            RuleFor(x => x.Name).Identical();
            RuleFor(x => x.Score).Identical();
        }
    }
}