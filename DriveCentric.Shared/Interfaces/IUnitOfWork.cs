using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace DriveCentric.Shared.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        #region Public Methods
        
        IRepository<T> GetRepository<T>() where T : class;
               

        void SetRepository<T>(IRepository<T> repository) where T : class;

        int SaveChanges();

        IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class;

        bool Any<T>(Expression<Func<T, bool>> predicate) where T : class;

        IQueryable<T> GetEntities<T>(params Expression<Func<T, object>>[] includes) where T : class;

        IQueryable GetEntities(Type type);

        T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class;

        void LoadReference<T, TElement>(T entity, Expression<Func<T, ICollection<TElement>>> collection) where TElement : class where T : class;

        void LoadReference<T, TElement>(T entity, Expression<Func<T, TElement>> property) where TElement : class where T : class;

        ICollection<TElement> GetReference<T, TElement>(T entity, Expression<Func<T, ICollection<TElement>>> collection) where TElement : class where T : class;

        TElement GetReference<T, TElement>(T entity, Expression<Func<T, TElement>> property) where TElement : class where T : class;

        ICollection<T> GetLocal<T>() where T : class;

        T Attach<T>(T entity) where T : class;

        T Create<T>() where T : class;

        void Insert<T>(T entity) where T : class;

        void Update<T>(T entity) where T : class;

        void Delete<T>(T entity) where T : class;

        void AddPostSaveAction(Action action);

        void AddBusinessLogic(object blObj);

        T GetBusinessLogic<T>() where T : class;

        object GetBusinessLogic(Type blType);

        T GetById<T>(Guid id) where T : class;

        T GetById<T>(Guid id, params Expression<Func<T, object>>[] includes) where T : class;

        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        void RollbackTransaction();

        bool CommitTransaction(); 

        #endregion Public Methods
    }
}
