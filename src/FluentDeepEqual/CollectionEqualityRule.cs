using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace FluentDeepEqual
{
    public class CollectionEqualityRule<TProperty, TKey> : PropertyRule
        where TKey : IComparable<TKey>, IComparable
    {
        protected readonly string _keyMemberName;
        protected readonly Func<TProperty, TKey> _keyExpressionCompiled;

        public CollectionEqualityRule(Expression<Func<TProperty, TKey>> keyExpression, MemberInfo member, Func<object, object> propertyFunc, LambdaExpression expression, Func<CascadeMode> cascadeModeThunk, Type typeToValidate, Type containerType) : base(member, propertyFunc, expression, cascadeModeThunk, typeToValidate, containerType)
        {
            if (keyExpression == null)
                throw new ArgumentNullException(nameof(keyExpression));

            var keyMember = keyExpression.GetMember();

            _keyMemberName = keyMember.Name;
            _keyExpressionCompiled = AccessorCache<TProperty>.GetCachedAccessor(member, keyExpression);
        }

        public static CollectionEqualityRule<TProperty, TKey> Create<T>(Expression<Func<T, IEnumerable<TProperty>>> expression, Expression<Func<TProperty, TKey>> keyExpression, Func<CascadeMode> cascadeModeThunk)
        {
            var member = expression.GetMember();
            var compiled = AccessorCache<T>.GetCachedAccessor(member, expression);

            return new CollectionEqualityRule<TProperty, TKey>(keyExpression, member, compiled.CoerceToNonGeneric(), expression, cascadeModeThunk, typeof(TProperty), typeof(T));
        }

        protected override IEnumerable<ValidationFailure> InvokePropertyValidator(ValidationContext context, IPropertyValidator validator, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                propertyName = InferPropertyName(Expression);
            }

            var propertyContext = new PropertyValidatorContext(context, this, propertyName);
            var results = new List<ValidationFailure>();
            var delegatingValidator = validator as IDelegatingValidator;

            if (delegatingValidator == null || delegatingValidator.CheckCondition(propertyContext))
            {
                var collectionPropertyValue = propertyContext.PropertyValue as IEnumerable<TProperty>;

                var otherCollection = GetOtherCollection(context, propertyContext);

                var preValidationResult = PreValidateCollections(propertyName, collectionPropertyValue, otherCollection);

                if (preValidationResult != null)
                    return preValidationResult;

                int count = 0;

                if (collectionPropertyValue != null)
                {
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        throw new InvalidOperationException("Could not automatically determine the property name ");
                    }

                    foreach (var element in collectionPropertyValue)
                    {
                        int index = count++;

                        string indexer = index.ToString();

                        var matchingValue = GetMatchingValue(propertyName, element, otherCollection, out ICollection<ValidationFailure> errors);

                        if (errors != null)
                            return errors;

                        var newContext = context.CloneForChildCollectionValidator(context.InstanceToValidate, preserveParentContext: true);
                        newContext.PropertyChain.Add(propertyName);
                        newContext.PropertyChain.AddIndexer(indexer, true);

                        newContext.RootContextData["_FDE_ComparisonSource"] = matchingValue;

                        var newPropertyContext = new PropertyValidatorContext(newContext, this, newContext.PropertyChain.ToString(), element);

                        results.AddRange(validator.Validate(newPropertyContext));
                    }
                }
            }

            return results;
        }


        protected override async Task<IEnumerable<ValidationFailure>> InvokePropertyValidatorAsync(ValidationContext context, IPropertyValidator validator, string propertyName, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                propertyName = InferPropertyName(Expression);
            }

            var propertyContext = new PropertyValidatorContext(context, this, propertyName);
            var delegatingValidator = validator as IDelegatingValidator;

            if (delegatingValidator == null || delegatingValidator.CheckCondition(propertyContext))
            {
                var collectionPropertyValue = propertyContext.PropertyValue as IEnumerable<TProperty>;

                var otherCollection = GetOtherCollection(context, propertyContext);

                var preValidationResult = PreValidateCollections(propertyName, collectionPropertyValue, otherCollection);

                if (preValidationResult != null)
                    return preValidationResult;

                if (collectionPropertyValue != null)
                {
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        throw new InvalidOperationException("Could not automatically determine the property name ");
                    }

                    var validatorTasks = collectionPropertyValue.Select(async (v, count) =>
                    {
                        string indexer = count.ToString();

                        var matchingValue = GetMatchingValue(propertyName, v, otherCollection, out ICollection<ValidationFailure> errors);

                        if (errors != null)
                            return errors;

                        var newContext = context.CloneForChildCollectionValidator(context.InstanceToValidate, preserveParentContext: true);
                        newContext.PropertyChain.Add(propertyName);
                        newContext.PropertyChain.AddIndexer(indexer, true);

                        newContext.RootContextData["_FDE_ComparisonSource"] = matchingValue;

                        var newPropertyContext = new PropertyValidatorContext(newContext, this, newContext.PropertyChain.ToString(), v);

                        return await validator.ValidateAsync(newPropertyContext, cancellation);
                    });

                    var results = new List<ValidationFailure>();

                    foreach (var task in validatorTasks)
                    {
                        var failures = await task;
                        results.AddRange(failures);
                    }

                    return results;
                }
            }

            return Enumerable.Empty<ValidationFailure>();
        }

        TProperty GetMatchingValue(string propertyName, TProperty collectionProperty, IEnumerable<TProperty> otherCollection, out ICollection<ValidationFailure> errors)
        {
            errors = null;

            var keyValue = _keyExpressionCompiled(collectionProperty);

            TProperty matchingValue = default;

            foreach (var otherItem in otherCollection)
            {
                TKey otherItemKeyValue = _keyExpressionCompiled(otherItem);

                if (keyValue.CompareTo(otherItemKeyValue) == 0)
                {
                    matchingValue = otherItem;

                    break;
                }
            }

            if (matchingValue == null)
            {
                errors = new List<ValidationFailure>();

                var msg = ValidatorOptions.LanguageManager.GetString("_FDE_ItemNotFoundInColl");

                errors.Add(new ValidationFailure(propertyName, msg, keyValue));
            }

            return matchingValue;
        }

        static IEnumerable<TProperty> GetOtherCollection(ValidationContext context, PropertyValidatorContext propertyContext)
        {
            var otherInstance = context.RootContextData["_FDE_ComparisonSource"];

            var otherCollectionObj = propertyContext.Rule.PropertyFunc(otherInstance);

            if (propertyContext.Rule.Transformer != null)
                otherCollectionObj = propertyContext.Rule.Transformer(otherCollectionObj);

            var otherCollection = otherCollectionObj as IEnumerable<TProperty>;

            return otherCollection;
        }

        private string InferPropertyName(LambdaExpression expression)
        {
            var paramExp = expression.Body as ParameterExpression;

            if (paramExp == null)
            {
                throw new InvalidOperationException("Could not infer property name for expression: " + expression + ". Please explicitly specify a property name by calling OverridePropertyName as part of the rule chain. Eg: RuleForEach(x => x).NotNull().OverridePropertyName(\"MyProperty\")");
            }

            return paramExp.Name;
        }

        static IEnumerable<ValidationFailure> PreValidateCollections(string propertyName, IEnumerable<TProperty> referenceCollection, IEnumerable<TProperty> otherCollection)
        {
            if (referenceCollection == null && otherCollection == null)
            {
                return Enumerable.Empty<ValidationFailure>();
            }
            else if (referenceCollection != null && otherCollection == null)
            {
                var errors = new List<ValidationFailure>();

                var msg = ValidatorOptions.LanguageManager.GetString("_FDE_Null");

                errors.Add(new ValidationFailure(propertyName, msg));

                return errors;
            }
            else if (referenceCollection.Count() != otherCollection.Count())
            {
                var errors = new List<ValidationFailure>();

                var msg = ValidatorOptions.LanguageManager.GetString("_FDE_CollCountMismatch");

                errors.Add(new ValidationFailure(propertyName, msg));

                return errors;
            }

            return null;
        }
    }
}