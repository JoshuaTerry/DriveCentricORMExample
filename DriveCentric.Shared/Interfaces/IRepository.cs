using DriveCentric.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DriveCentric.Shared.Interfaces
{
    /// <summary>
    /// Defines the required functionality for a class to act as a "Repository" by accessing and
    /// returning entities from a data store.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        #region Public Properties

        IQueryable<T> Entities { get; }

        ISQLUtilities Utilities { get; }

        #endregion Public Properties

        #region Public Methods

        EntityState GetEntityState(T entity);

        void Delete(T entity);

        T GetById(Guid id);

        T GetById(Guid id, params Expression<Func<T, object>>[] includes);

        T Find(params object[] keyValues);

        T Create();

        T Insert(T entity);

        T Update(T entity);

        void UpdateChangedProperties(Guid id, IDictionary<string, object> propertyValues, Action<T> action = null);

        void UpdateChangedProperties(T entity, IDictionary<string, object> propertyValues, Action<T> action = null);

        List<string> GetModifiedProperties(T entity);

        TElement GetOriginalPropertyValue<TElement>(T entity, Expression<Func<T, TElement>> property) where TElement : class;

        void LoadReference<TElement>(T entity, Expression<Func<T, ICollection<TElement>>> collection) where TElement : class;

        void LoadReference<TElement>(T entity, Expression<Func<T, TElement>> property) where TElement : class;

        ICollection<TElement> GetReference<TElement>(T entity, Expression<Func<T, ICollection<TElement>>> collection) where TElement : class;

        TElement GetReference<TElement>(T entity, Expression<Func<T, TElement>> property) where TElement : class;

        ICollection<T> GetLocal();

        T Attach(T entity);

        IQueryable<T> GetEntities(params Expression<Func<T, object>>[] includes);

        #endregion Public Methods
    }

    

    // Defines non-generic versions of properties and methods.  (These are added only if needed.)
    public interface IRepository
    {
        IQueryable Entities { get; }
    }
}
