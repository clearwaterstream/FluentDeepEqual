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
        public void Basic()
        {

        }
    }

    class StudentsComparator : CollectionEqualityValidator<Student, string>
    {
        public StudentsComparator() : base(x => x.Name)
        {
            RuleFor(x => x.Name).Identical();
            RuleFor(x => x.Score).Identical();
        }
    }

    class ClassComparator : EqualityValidator<Class>
    {
        public ClassComparator()
        {
            RuleFor(x => x.Subject).Identical();
        }
    }
}
