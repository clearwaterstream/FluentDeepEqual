using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentDeepEqual
{
    public static class IdenticalnessValidatorExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> Identical<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
            where TProperty : IComparable<TProperty>, IComparable
        {
            var validator = new IdenticalnessValidator();

            return ruleBuilder.SetValidator(validator);
        }

        public static IRuleBuilderOptions<T, TProperty?> Identical<T, TProperty>(this IRuleBuilder<T, TProperty?> ruleBuilder)
            where TProperty : struct, IComparable<TProperty>, IComparable
        {
            var validator = new IdenticalnessValidator();

            return ruleBuilder.SetValidator(validator);
        }
    }
}
