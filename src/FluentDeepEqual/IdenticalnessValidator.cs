using FluentValidation.Resources;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentDeepEqual
{
    public class IdenticalnessValidator<TProperty> : PropertyValidator
        where TProperty : IComparable<TProperty>, IComparable
    {
        static IdenticalnessValidator()
        {
            Initializer.NoOp();
        }

        public IdenticalnessValidator() : base(new LanguageStringSource(nameof(EqualValidator)))
        {

        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            object source = context.ParentContext.RootContextData["comparison_source"];

            var referenceVal = (TProperty)context.Rule.PropertyFunc(source);

            var compareWith = (TProperty)context.PropertyValue;

            bool success = false;

            if (referenceVal == null && compareWith == null)
            {
                success = true;
            }
            else if (referenceVal != null && compareWith == null)
            {
                success = false;
            }
            else
            {
                success = referenceVal.CompareTo(compareWith) == 0;
            }

            if (!success)
            {
                context.MessageFormatter.AppendArgument("ComparisonValue", referenceVal);

                return false;
            }

            return true;
        }
    }
}
