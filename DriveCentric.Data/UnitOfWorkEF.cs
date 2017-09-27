using DriveCentric.Logger;
using DriveCentric.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace DriveCentric.Data
{
    public class UnitOfWorkEF : IUnitOfWork, IDisposable
    {
        #region Private Fields

        private readonly ILogger logger = LoggerManager.GetLogger(typeof(UnitOfWorkEF));
        private DomainContext clientContext; 
        private bool isDisposed = false;
        private Dictionary<Type, object> repositories;
        private List<object> businessLogic; 
        private DbContextTransaction dbTransaction;
        private static object lockObject;
        private List<Action> saveActions;

        #endregion Private Fields


        #region Public Constructors

        public UnitOfWorkEF() : this(null)
        {

        }

        public UnitOfWorkEF(DbContext context)
        {
            clientContext = (DomainContext)context; 
            repositories = new Dictionary<Type, object>(); 
            businessLogic = new List<object>(); 
            dbTransaction = null;
            saveActions = new List<Action>();
        }

        static UnitOfWorkEF()
        {
            lockObject = new object();
        }

        #endregion Public Constructors
         

        #region Public Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetRepository<T>(IRepository<T> repository) where T : class
        {
            repositories[typeof(T)] = repository;
        }

        /// <summary>
        /// Return a queryable collection of entities filtered by a predicate.
        /// </summary>
        public IQueryable<T> Where<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
        {
            return GetRepository<T>().Entities.Where(predicate);
        }

        /// <summary>
        /// Determine if any entries exist in a collection of entities filtered by a predicate.
        /// </summary>
        public bool Any<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
        {
            return GetRepository<T>().Entities.Any(predicate);
        }

        /// <summary>
        /// Return a queryable collection of entities.
        /// </summary>
        public IQueryable<T> GetEntities<T>(params Expression<Func<T, object>>[] includes) where T : class
        {
            return GetRepository<T>().GetEntities(includes);
        }

        /// <summary>
        /// Return a queryable collection of entities.
        /// </summary>
        public IQueryable GetEntities(Type type)
        {
            return GetContext(type).Set(type);
        }

        /// <summary>
        /// Returns the first entity that satisfies a condition or null if no such entity is found.
        /// </summary>
        public T FirstOrDefault<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
        {
            return GetRepository<T>().Entities.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Explicitly load a reference property or collection for an entity.
        /// </summary>
        public void LoadReference<T, TElement>(T entity, System.Linq.Expressions.Expression<Func<T, ICollection<TElement>>> collection) where TElement : class where T : class
        {
            GetRepository<T>().LoadReference(entity, collection);
        }

        /// <summary>
        /// Explicitly load a reference property or collection for an entity.
        /// </summary>
        public void LoadReference<T, TElement>(T entity, System.Linq.Expressions.Expression<Func<T, TElement>> property) where TElement : class where T : class
        {
            GetRepository<T>().LoadReference(entity, property);
        }

        /// <summary>
        /// Explicitly load a reference property or collection for an entity and return the value.
        /// </summary>
        public ICollection<TElement> GetReference<T, TElement>(T entity, System.Linq.Expressions.Expression<Func<T, ICollection<TElement>>> collection) where TElement : class where T : class
        {
            return GetRepository<T>().GetReference<TElement>(entity, collection);
        }

        /// <summary>
        /// Explicitly load a reference property or collection for an entity and return the value.
        /// </summary>
        public TElement GetReference<T, TElement>(T entity, System.Linq.Expressions.Expression<Func<T, TElement>> property) where TElement : class where T : class
        {
            try
            {
                return GetRepository<T>().GetReference<TElement>(entity, property);
            }
            catch
            {
                logger.LogError($"GetReference on type {typeof(T).Name} failed for {property.Name}.");
                return null;
            }
        }

        /// <summary>
        /// Return a collection of entities that have already been loaded or added to the repository.
        /// </summary>
        public ICollection<T> GetLocal<T>() where T : class
        {
            return GetRepository<T>().GetLocal();
        }

        /// <summary>
        /// Attach an entity (which may belong to another context) to the unit of work.
        /// </summary>
        public T Attach<T>(T entity) where T : class
        {
            if (entity != null)
            {
                return GetRepository<T>().Attach(entity);
            }
            return null;
        }

        public T Create<T>() where T : class
        {
            return GetRepository<T>().Create();
        }

        public void Insert<T>(T entity) where T : class
        {
            GetRepository<T>().Insert(entity);
        }

        public void Update<T>(T entity) where T : class
        {
            GetRepository<T>().Update(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            GetRepository<T>().Delete(entity);
        }

        public T GetById<T>(Guid id) where T : class
        {
            return GetRepository<T>().GetById(id);
        }

        public T GetById<T>(Guid id, params Expression<Func<T, object>>[] includes) where T : class
        {
            return GetRepository<T>().GetById(id, includes);
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            IRepository<T> repository = null;

            var type = typeof(T);

            if (!repositories.ContainsKey(type))
            {
                DbContext context = GetContext(type);

                // Create a repository, then add it to the dictionary.
                repository = new Repository<T>(context);

                repositories.Add(type, repository);
            }
            else
            {
                // Repository already exists...
                repository = repositories[type] as IRepository<T>;
            }

            return repository;
        }


        /// <summary>
        /// Get (create if necessary) the correct DbContext for a given entity type.
        /// </summary>
        private DbContext GetContext(Type type)
        {
            return clientContext ?? new DomainContext();  
        }

        /// <summary>
        /// Saves all changes made to the unit of work to the database.
        /// </summary>
        public int SaveChanges()
        {
            return SaveChanges(true);
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            lock (lockObject)
            {
                if (dbTransaction != null)
                {
                    throw new InvalidOperationException("Cannot begin a new transcation because a transaction has already been started.");
                }
                if (clientContext == null)
                {
                    clientContext = new DomainContext();
                }
                dbTransaction = clientContext.Database.BeginTransaction(isolationLevel);
            }
        }

        public void RollbackTransaction()
        {
            lock (lockObject)
            {
                dbTransaction?.Rollback();
                dbTransaction?.Dispose();
                dbTransaction = null;
            }
        }

        public bool CommitTransaction()
        {
            bool success = true;

            lock (lockObject)
            {
                try
                {
                    SaveChanges(false);
                    dbTransaction?.Commit();
                    dbTransaction?.Dispose();
                    dbTransaction = null;
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException)
                {
                    dbTransaction = clientContext.Database.CurrentTransaction;
                    success = false;
                }
            }

            if (success)
            {
                PerformSaveActions();
            }
            saveActions.Clear();
            return success;
        }

        /// <summary>
        /// Add an action to be performed after SaveChanges completes.
        /// </summary>
        public void AddPostSaveAction(Action action)
        {
            saveActions.Add(action);
        }

        public void AddBusinessLogic(object logic)
        {
            if (!businessLogic.Contains(logic))
                businessLogic.Add(logic);
        }

        /// <summary>
        /// Get (or create) a business logic instance associated with this unit of work.
        /// </summary>
        /// <typeparam name="T">Business logic type</typeparam>
        public T GetBusinessLogic<T>() where T : class
        {
            return GetBusinessLogic(typeof(T)) as T;
        }

        /// <summary>
        /// Get (or create) a business logic instance associated with this unit of work.
        /// </summary>
        /// <param name="logicType">Business logic type</param>
        public object GetBusinessLogic(Type logicType)
        {
            object logic = businessLogic.FirstOrDefault(p => p.GetType() == logicType);
            if (logic == null)
            {
                logic = Activator.CreateInstance(logicType, this);
                AddBusinessLogic(logic);
            }
            return logic;
        }
                     
        public IList<T> GetRepositoryDataSource<T>() where T : class
        {
            return GetEntities<T>().ToList();
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    dbTransaction?.Rollback();
                    dbTransaction?.Dispose();
                    clientContext?.Dispose(); 
                }

                isDisposed = true;
            }
        }

        #endregion Protected Methods

        #region Private Methods


        private int SaveChanges(bool performSaveActions)
        {
            int result = 0;

            result = clientContext?.SaveChanges() ?? 0;

            if (performSaveActions)
            {
                PerformSaveActions();
            }

            return result;
        }

        private void PerformSaveActions()
        {
            saveActions.ForEach(p => p.Invoke());
            saveActions.Clear();
        }

        #endregion
    }
}
