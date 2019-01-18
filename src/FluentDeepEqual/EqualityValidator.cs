using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentDeepEqual
{
    public abstract class EqualityValidator<T> : AbstractValidator<T>
    {
        static EqualityValidator()
        {
            Initializer.NoOp();
        }

        public ValidationResult Compare(T reference, T other)
        {
            if (reference == null && other == null)
            {
                return new ValidationResult(); // all good, both are null
            }
            else if (reference == null || other == null) // one null and the other is not -- no point doing a deeper comparison
            {
                var propName = reference == null ? $"${nameof(reference)}" : $"${nameof(other)}";

                var result = new ValidationResult();

                var msg = ValidatorOptions.LanguageManager.GetString("_FDE_Null");

                result.Errors.Add(new ValidationFailure(propName, msg));

                return result;
            }

            var validationContext = new ValidationContext<T>(other);
            validationContext.RootContextData["_FDE_ComparisonSource"] = reference;

            return Validate(validationContext);
        }
    }
}
