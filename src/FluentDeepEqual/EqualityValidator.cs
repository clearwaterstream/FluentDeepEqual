using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
                var result = new ValidationResult();

                var msg = ValidatorOptions.LanguageManager.GetString("_FDE_Null");

                result.Errors.Add(new ValidationFailure(null, msg));

                return result;
            }

            var validationContext = new ValidationContext<T>(other);
            validationContext.RootContextData["_FDE_ComparisonSource"] = reference;

            return Validate(validationContext);
        }

        public IRuleBuilderInitialCollection<T, TProperty> RuleForCollection<TProperty, TKey>(Expression<Func<T, IEnumerable<TProperty>>> expression, Expression<Func<TProperty, TKey>> keyExpression)
            where TKey : IComparable<TKey>, IComparable
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var rule = CollectionEqualityRule<TProperty, TKey>.Create(expression, keyExpression, () => CascadeMode);

            AddRule(rule);

            var ruleBuilder = new RuleBuilder<T, TProperty>(rule, this);

            return ruleBuilder;
        }
    }
}
