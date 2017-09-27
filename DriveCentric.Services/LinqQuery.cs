using DriveCentric.Shared.Extensions;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DriveCentric.Services
{
    public class LinqQuery<TEntity>
    {
        #region Private Fields

        private bool hasPredicate = false;
        private ExpressionStarter<TEntity> predicate = PredicateBuilder.New<TEntity>();

        private List<string> orderBy = new List<string>();

        public List<string> OrderBy
        {
            get { return orderBy; }
            set { orderBy = value; }
        }

        private IQueryable<TEntity> query;

        #endregion Private Fields

        #region Public Properties

        public int Offset { get; set; }
        public int Limit { get; set; }

        public ExpressionStarter<TEntity> Predicate
        {
            get { return predicate; }
        }

        #endregion Public Properties

        #region Public Constructors

        public LinqQuery(IQueryable<TEntity> query)
        {
            this.query = query;
        }

        #endregion Public Constructors

        #region Public Methods

        public IQueryable<TEntity> GetQueryable()
        {
            var query = this.query?.AsExpandable();
            if (hasPredicate)
            {
                query = query.Where(predicate);
            }
            query = AddSorting(query);
            query = AddPaging(query);

            return query;
        }

        public LinqQuery<TEntity> Or(Expression<Func<TEntity, bool>> expression)
        {
            if (expression != null)
            {
                hasPredicate = true;
                predicate = predicate.Or(expression);
            }

            return this;
        }

        #endregion Public Methods

        #region Protected Methods

        protected void PredicateAnd(Expression<Func<TEntity, bool>> expression)
        {
            if (expression != null)
            {
                hasPredicate = true;
                predicate = predicate.And(expression);
            }
        }

        #endregion Protected Methods

        private IQueryable<TEntity> AddPaging(IQueryable<TEntity> query)
        {
            if (Offset > 0)
            {
                query = query.Skip(Offset * Limit);
            }
            if (Limit > 0)
            {
                query = query.Take(Limit);
            }
            return query;
        }

        private IQueryable<TEntity> AddSorting(IQueryable<TEntity> query)
        {
            if (orderBy?.Count > 0)
            {
                int orderNumber = 0;
                foreach (string orderByColumn in orderBy)
                {
                    orderNumber++;
                    string propertyName = orderByColumn;
                    bool descending = false;
                    if (propertyName.StartsWith("-"))
                    {
                        descending = true;
                        propertyName = propertyName.TrimStart('-');
                    }
                    query = query.DynamicOrderBy(propertyName, descending, orderNumber == 1);
                }
            }
            else if (Offset > 0 || Limit > 0)
            {
                query = query.DynamicOrderBy("Id", true, true);
            }
            return query;
        }
    }
}
