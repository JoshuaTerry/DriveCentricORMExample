using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DriveCentric.Services
{
    public class CriteriaQuery<TDatabaseType, TSearchModel> : LinqQuery<TDatabaseType>
    {
        private TSearchModel model;
        public TSearchModel Model => model;

        public CriteriaQuery(IQueryable<TDatabaseType> query, TSearchModel model)
            : base(query)
        {
            this.model = model;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> And(Expression<Func<TDatabaseType, bool>> expression)
        {
            if (expression == null)
            {
                return this;
            }

            PredicateAnd(expression);

            return this;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> IfModelPropertyIsNotBlankAndDatabaseFieldIsGreaterThanOrEqualToIt<TModelPropertyType>(Expression<Func<TSearchModel, TModelPropertyType>> test, Expression<Func<TDatabaseType, TModelPropertyType>> expression)
        {
            var testValue = GetTestValue(test);

            if (testValue != null && !(testValue.Equals(default(TModelPropertyType))))
            {
                var constantExpression = Expression.Constant(testValue, typeof(TModelPropertyType));
                var compExpression = Expression.GreaterThanOrEqual(expression.Body, constantExpression);
                var lambda = (Expression<Func<TDatabaseType, bool>>)Expression.Lambda(compExpression, expression.Parameters[0]);

                PredicateAnd(lambda);
            }

            return this;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> IfModelPropertyIsNotBlankAndDatabaseFieldIsLessThanOrEqualToIt<TModelPropertyType>(Expression<Func<TSearchModel, TModelPropertyType>> test, Expression<Func<TDatabaseType, TModelPropertyType>> expression)
        {
            var testValue = GetTestValue(test);

            if (testValue != null && !(testValue.Equals(default(TModelPropertyType))))
            {
                var constantExpression = Expression.Constant(testValue, typeof(TModelPropertyType));
                var compExpression = Expression.LessThanOrEqual(expression.Body, constantExpression);
                var lambda = (Expression<Func<TDatabaseType, bool>>)Expression.Lambda(compExpression, expression.Parameters[0]);

                PredicateAnd(lambda);
            }

            return this;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> IfModelPropertiesAreNotBlankAndDatabaseFieldIsBetweenThem<TModelPropertyType>(Expression<Func<TSearchModel, TModelPropertyType>> lowerBoundTest, Expression<Func<TSearchModel, TModelPropertyType>> upperBoundTest, Expression<Func<TDatabaseType, TModelPropertyType>> expression)
        {
            IfModelPropertyIsNotBlankAndDatabaseFieldIsGreaterThanOrEqualToIt(lowerBoundTest, expression);
            IfModelPropertyIsNotBlankAndDatabaseFieldIsLessThanOrEqualToIt(upperBoundTest, expression);
            return this;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> IfModelPropertyIsNotBlankAndDatabaseContainsIt<TModelPropertyType>(Expression<Func<TSearchModel, TModelPropertyType>> test, Expression<Func<TDatabaseType, TModelPropertyType>> expression)
        {
            IfCheckMethod("Contains", test, expression);
            return this;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> IfModelPropertyIsNotBlankAndItEqualsDatabaseField<TModelPropertyType>(Expression<Func<TSearchModel, TModelPropertyType>> test, Expression<Func<TDatabaseType, TModelPropertyType>> expression)
        {
            //if (HasValue(test))
            var testValue = GetTestValue(test);

            if (!IsNullOrBlankOrDefault<TModelPropertyType>(testValue))
            {
                var constantExpression = Expression.Constant(testValue, typeof(TModelPropertyType));
                var equalExpression = Expression.Equal(expression.Body, constantExpression);
                var lambda = (Expression<Func<TDatabaseType, bool>>)Expression.Lambda(equalExpression, expression.Parameters[0]);

                PredicateAnd(lambda);
            }

            return this;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> IfModelPropertyIsNotBlankThenAndTheExpression<TModelPropertyType>(Expression<Func<TSearchModel, TModelPropertyType>> test, Expression<Func<TDatabaseType, bool>> expression)
        {
            var testValue = test.Compile().Invoke(Model);
            if (testValue != null && !(testValue.Equals(default(TModelPropertyType))))
            {
                PredicateAnd(expression);
            }

            return this;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> IfModelPropertyIsNotBlankAndDatabaseFieldStartsWithIt<TModelPropertyType>(Expression<Func<TSearchModel, TModelPropertyType>> test, Expression<Func<TDatabaseType, TModelPropertyType>> expression)
        {
            IfCheckMethod("StartsWith", test, expression);
            return this;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> IfModelExpressionIsTrueThenAndTheExpression(Expression<Func<TSearchModel, bool>> condition, Expression<Func<TDatabaseType, bool>> expression)
        {
            if (condition.Compile().Invoke(Model))
            {
                And(expression);
            }

            return this;
        }

        public bool IsNull(Expression<Func<TSearchModel, bool>> propertyExpression)
        {
            return propertyExpression.Compile().Invoke(Model);
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> SetOffset(int? offset)
        {
            Offset = offset ?? 0;
            return this;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> SetLimit(int? limit)
        {
            Limit = limit ?? 0;
            return this;
        }

        public CriteriaQuery<TDatabaseType, TSearchModel> SetOrderBy(string orderBy)
        {
            var orderByList = orderBy?.Split(',').ToList();
            OrderBy = orderByList;
            return this;
        }

        private TPropertyType GetTestValue<TPropertyType>(Expression<Func<TSearchModel, TPropertyType>> test)
        {
            TPropertyType testValue;

            try
            {
                testValue = test.Compile().Invoke(Model);
            }
            catch
            {
                testValue = default(TPropertyType);
            }

            return testValue;
        }

        private CriteriaQuery<TDatabaseType, TSearchModel> IfCheckMethod<TPropertyType>(string method, Expression<Func<TSearchModel, TPropertyType>> test, Expression<Func<TDatabaseType, TPropertyType>> expression)
        {
            var testValue = GetTestValue(test);

            if (!IsNullOrBlankOrDefault<TPropertyType>(testValue))
            {
                var constantExpression = Expression.Constant(testValue, typeof(TPropertyType));
                MethodInfo contains = typeof(TPropertyType).GetMethod(method, new Type[] { typeof(TPropertyType) });
                var expressionBody = Expression.Call(expression.Body, contains, constantExpression);
                var lambda = (Expression<Func<TDatabaseType, bool>>)Expression.Lambda(expressionBody, expression.Parameters[0]);

                PredicateAnd(lambda);
            }

            return this;
        }

        private bool IsNullOrBlankOrDefault<TType>(object testValue)
        {
            if (testValue == null)
                return true;
            if (testValue.Equals(default(TType)))
                return true;
            if (testValue is string && string.IsNullOrWhiteSpace((string)testValue))
                return true;
            if (testValue is Guid && (Guid.Empty.ToString() == testValue.ToString()))
                return true;
            return false;
        }
    }
}
