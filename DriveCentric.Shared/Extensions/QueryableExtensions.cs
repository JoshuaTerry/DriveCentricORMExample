using System;
using System.Linq;
using System.Linq.Expressions;

namespace DriveCentric.Shared.Extensions
{
    public static class QueryableExtensions
    {
        const string DESCENDING = "Descending";
        const string ORDER_BY = "OrderBy";
        const string THEN_BY = "ThenBy";

        public static IQueryable<TEntity> DynamicOrderBy<TEntity>(this IQueryable<TEntity> query, string orderByProperty, bool isDescending, bool isFirstOrder)
        {
            try
            {
                string order = isFirstOrder ? ORDER_BY : THEN_BY;
                string descending = isDescending ? DESCENDING : string.Empty;
                string command = $"{order}{descending}";
                var type = typeof(TEntity);
                var property = type.GetProperty(orderByProperty);
                var parameter = Expression.Parameter(type, "p");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExpression = Expression.Lambda(propertyAccess, parameter);
                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    command,
                    new Type[] { type, property.PropertyType },
                    query.Expression, Expression.Quote(orderByExpression));
                return query.Provider.CreateQuery<TEntity>(resultExpression);

            }
            catch (Exception)
            {
                //If there is any exception trying to order by this property, just don't order by
                return query;
            }
        }


    }
}
