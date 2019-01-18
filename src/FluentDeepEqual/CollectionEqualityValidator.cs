using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FluentDeepEqual
{
    /// <summary>
    /// Compares two collections. It's assumed that both collections have non-repeating items identified by an id or some other unique field
    /// </summary>
    public abstract class CollectionEqualityValidator<T, TKey> : AbstractValidator<T>
        where TKey : IComparable<TKey>, IComparable
    {
        protected readonly string _keyMemberName;
        protected readonly Func<T, TKey> _keyExpressionCompiled;

        static CollectionEqualityValidator()
        {
            Initializer.NoOp();
        }

        /// <param name="keyExpression">An id or other field that can act as unique identifier for the object</param>
        protected CollectionEqualityValidator(Expression<Func<T, TKey>> keyExpression)
        {
            if (keyExpression == null)
                throw new ArgumentNullException(nameof(keyExpression));

            var member = keyExpression.GetMember();

            _keyMemberName = member.Name;
            _keyExpressionCompiled = AccessorCache<T>.GetCachedAccessor(member, keyExpression);
        }

        public ValidationResult Compare(IEnumerable<T> referenceCollection, IEnumerable<T> otherCollection)
        {
            ValidationResult result;

            if (referenceCollection == null && otherCollection == null)
            {
                return new ValidationResult();  // all good, both are null
            }
            else if (referenceCollection == null || otherCollection == null)
            {
                var propName = referenceCollection == null ? $"${nameof(referenceCollection)}" : $"${nameof(otherCollection)}";

                result = new ValidationResult();

                var msg = ValidatorOptions.LanguageManager.GetString("_FDE_Null");

                result.Errors.Add(new ValidationFailure(propName, msg));

                return result;
            }
            else if (referenceCollection.Count() != otherCollection.Count())
            {
                result = new ValidationResult();

                var msg = ValidatorOptions.LanguageManager.GetString("_FDE_CollCountMismatch");

                result.Errors.Add(new ValidationFailure("$count", msg));

                return result;
            }

            var errors = new List<ValidationFailure>();

            foreach (var item in referenceCollection)
            {
                var keyValue = _keyExpressionCompiled(item);
                T matchingValue = default;

                foreach (var otherItem in otherCollection)
                {
                    TKey otherItemKeyValue = _keyExpressionCompiled(otherItem);

                    if (keyValue.CompareTo(otherItemKeyValue) == 0)
                    {
                        matchingValue = otherItem;

                        break;
                    }
                }

                if (matchingValue != null)
                {
                    var validationContext = new ValidationContext<T>(matchingValue);

                    validationContext.RootContextData["_FDE_ComparisonSource"] = item;

                    var resultPartial = Validate(validationContext);

                    if (resultPartial.Errors.Count > 0)
                    {
                        errors.AddRange(resultPartial.Errors);
                    }
                }
                else
                {
                    var msg = ValidatorOptions.LanguageManager.GetString("_FDE_ItemNotFoundInColl");

                    errors.Add(new ValidationFailure(_keyMemberName, msg, keyValue));
                }
            }

            result = new ValidationResult(errors);

            return result;
        }
    }
}
