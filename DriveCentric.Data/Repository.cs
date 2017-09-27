using DriveCentric.Logger;
using DriveCentric.Shared.Helpers;
using DriveCentric.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace DriveCentric.Data
{
    public class Repository<T> : IRepository<T>, IRepository where T : class
    {
        #region Private Fields

        private readonly DbContext context = null;
        private IDbSet<T> entities = null;
        private SQLUtilities utilities = null;
        private bool isUOW = false;
        private ICollection<T> local = null;
        private readonly ILogger logger = LoggerManager.GetLogger(typeof(Repository<T>));
        #endregion Private Fields

        #region Public Properties
        protected ILogger Logger => logger;

        public virtual IQueryable<T> Entities => EntitySet;

        IQueryable IRepository.Entities => EntitySet;

        public ISQLUtilities Utilities
        {
            get
            {
                if (utilities == null && context != null)
                {
                    utilities = new SQLUtilities(context);
                }

                return utilities;
            }
        }

        #endregion Public Properties

        #region Protected Properties

        protected IDbSet<T> EntitySet
        {
            get
            {
                if (entities == null)
                {
                    entities = context.Set<T>();

                }

                return entities;
            }
        }

        #endregion Protected Properties

        #region Public Constructors

        public Repository() : this(new DomainContext())
        {
            isUOW = false;
        }

        public Repository(DbContext context)
        {
            this.context = context;
            isUOW = (context != null);
        }
        #endregion Public Constructors

        #region Public Methods       

        public DriveCentric.Shared.Enums.EntityState GetEntityState(T entity)
        {
            DbEntityEntry<T> entry = context.Entry(entity);

            return (DriveCentric.Shared.Enums.EntityState)entry.State;
        }

        public virtual void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Attach(entity);
        }

        /// <summary>
        /// Explicitly load a reference property or collection for an entity.
        /// </summary>
        public void LoadReference<TElement>(T entity, System.Linq.Expressions.Expression<Func<T, ICollection<TElement>>> collection) where TElement : class
        {
            GetReference<TElement>(entity, collection);
        }

        /// <summary>
        /// Explicitly load a reference property or collection for an entity.
        /// </summary>
        public void LoadReference<TElement>(T entity, System.Linq.Expressions.Expression<Func<T, TElement>> property) where TElement : class
        {
            GetReference<TElement>(entity, property);
        }

        /// <summary>
        /// Explicitly load a reference property or collection for an entity and return the value.
        /// </summary>
        public ICollection<TElement> GetReference<TElement>(T entity, System.Linq.Expressions.Expression<Func<T, ICollection<TElement>>> collection) where TElement : class
        {
            var entry = context.Entry(entity);

            if (entry.State == System.Data.Entity.EntityState.Detached || entry.State == System.Data.Entity.EntityState.Added)
            {
                var method = collection.Compile();
                return method.Invoke(entity) ?? new List<TElement>();
            }

            var entryCollection = entry.Collection(collection);
            if (!entryCollection.IsLoaded)
                entryCollection.Load();
            return entryCollection.CurrentValue;
        }

        /// <summary>
        /// Explicitly load a reference property or collection for an entity and return the value.
        /// </summary>
        public TElement GetReference<TElement>(T entity, System.Linq.Expressions.Expression<Func<T, TElement>> property) where TElement : class
        {
            var entry = context.Entry(entity);
            if (entry.State == System.Data.Entity.EntityState.Detached || entry.State == System.Data.Entity.EntityState.Added)
            {
                var method = property.Compile();
                TElement returnValue = method.Invoke(entity);
                if (returnValue != null || entry.State != System.Data.Entity.EntityState.Added)
                {
                    return returnValue;
                }
            }
            var reference = entry.Reference(property);
            if (!reference.IsLoaded)
                reference.Load();
            return reference.CurrentValue;
        }

        /// <summary>
        /// Return a collection of entities that have already been loaded or added to the repository.
        /// </summary>
        public ICollection<T> GetLocal()
        {
            if (local == null)
            {
                local = EntitySet.Local;
            }
            return local;
        }

        /// <summary>
        /// Attach an entity (which may belong to another context) to the repository.
        /// </summary>
        public T Attach(T entity)
        {
            return Attach(entity, System.Data.Entity.EntityState.Unchanged);
        }

        public T Find(params object[] keyValues) => EntitySet.Find(keyValues);

        public IQueryable<T> GetEntities(params Expression<Func<T, object>>[] includes)
        {
            if (includes == null || includes.Length == 0)
            {
                return Entities;
            }

            var query = context.Set<T>().AsQueryable();

            foreach (Expression<Func<T, object>> include in includes)
            {
                string name = PathHelper.NameFor(include, true);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Include(name);
                }
            }

            return query;
        }

        public T GetById(Guid id) => EntitySet.Find(id);

        public T GetById(Guid id, params Expression<Func<T, object>>[] includes)
        {
            if (typeof(IEntity).IsAssignableFrom(typeof(T)))
            {
                var query = (IQueryable<IEntity>)GetEntities(includes);
                return query.FirstOrDefault(p => p.Id == id) as T;
            }
            else
            {
                return GetById(id);
            }
        }

        public virtual T Create()
        {
            T entity = Activator.CreateInstance<T>(); // ...to avoid adding the new() generic type restriction.          
            EntitySet.Add(entity);

            return entity;
        }

        public virtual T Insert(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (context.Entry(entity).State != System.Data.Entity.EntityState.Added)
            {
                EntitySet.Add(entity);
            }

            return entity;
        }

        public virtual T Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Attach(entity, System.Data.Entity.EntityState.Modified);
            context.Entry(entity).State = System.Data.Entity.EntityState.Modified;

            return entity;
        }

        public virtual void UpdateChangedProperties(Guid id, IDictionary<string, object> propertyValues, Action<T> action = null)
        {
            UpdateChangedProperties(GetById(id), propertyValues, action);
        }

        public virtual void UpdateChangedProperties(T entity, IDictionary<string, object> propertyValues, Action<T> action = null)
        {
            DbEntityEntry<T> entry = context.Entry(entity);
            DbPropertyValues currentValues = entry.CurrentValues;
            IEnumerable<string> propertynames = currentValues.PropertyNames;
             
            foreach (KeyValuePair<string, object> keyValue in propertyValues)
            {
                if (propertynames.Contains(keyValue.Key))
                {
                    currentValues[keyValue.Key] = keyValue.Value;
                }
                else
                {
                    // NotMapped property: Use reflection to try and set the property in the entity.
                    typeof(T).GetProperty(keyValue.Key)?.SetValue(entity, keyValue.Value);
                }
            }

            action?.Invoke(entity); 
        }

        public List<string> GetModifiedProperties(T entity)
        {
            var list = new List<string>();
            DbEntityEntry<T> entry = context.Entry(entity);

            if (entry.State != System.Data.Entity.EntityState.Detached)
            {
                foreach (string property in entry.OriginalValues.PropertyNames)
                {
                    if (entry.Property(property).IsModified)
                    {
                        list.Add(property);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Get the original property value for an entity.
        /// </summary>
        public TElement GetOriginalPropertyValue<TElement>(T entity, Expression<Func<T, TElement>> property) where TElement : class
        {
            DbEntityEntry<T> entry = context.Entry(entity);
            if (entry.State == System.Data.Entity.EntityState.Detached || entry.State == System.Data.Entity.EntityState.Added)
            {
                return null;
            }
            string propertyName = (property.Body as MemberExpression).Member.Name;
            return entry.OriginalValues.GetValue<TElement>(propertyName);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Attach an entity to the context.  If it's already loaded, return the loaded entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityState"></param>
        /// <returns></returns>
        private T Attach(T entity, System.Data.Entity.EntityState entityState)
        {
            if (entity == null)
            {
                return null;
            }

            DbEntityEntry<T> entityEntry = context.Entry(entity);
            if (entityEntry.State == entityState)
            {
                return entity;
            }

            if (entity != null && entityEntry.State == System.Data.Entity.EntityState.Detached)
            {
                if (entity is IEntity)
                {
                    // Get the entity's Id and look for an already loaded instance in EntitySet.Local.  If found, return the loaded entity.
                    Guid id = ((IEntity)entity).Id;
                    IEntity existing = EntitySet.Local.Cast<IEntity>().FirstOrDefault(p => p.Id == id);
                    if (existing != null)
                    {
                        return existing as T;
                    }
                }
            }

            // EF makes attaching an object from another context very painful if that object contains other referenced entities.  Attach() can throw an exception if 
            // any of the referenced entities were previously loaded into the context.

            // The first step is capture the state of all tracked entites in the context by storing them in a dictionary (keyed by entity.Id)
            var stateDict = new Dictionary<Guid, System.Data.Entity.EntityState>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is IEntity)
                {
                    stateDict[((IEntity)entry.Entity).Id] = entry.State;
                }
            }

            // Then add the entity via the Add method.  
            EntitySet.Add(entity);

            // Change the entity state to what we want it to be (Unmodified, or Modified)
            context.Entry(entity).State = entityState;

            // Finally check the state of all tracked entities, looking for ones that are in an Added State.
            foreach (var entry in context.ChangeTracker.Entries().Where(p => p.State == System.Data.Entity.EntityState.Added && p.Entity is IEntity))
            {
                System.Data.Entity.EntityState state;

                if (stateDict.TryGetValue(((IEntity)entry.Entity).Id, out state))
                {
                    // If the entity was already being tracked and wasn't originally in the Added state, detach the entity.
                    if (state != System.Data.Entity.EntityState.Added)
                    {
                        context.Entry(entry.Entity).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    // If the entity wasn't being tracked, make sure it's state is Unchanged.
                    context.Entry(entry.Entity).State = System.Data.Entity.EntityState.Unchanged;
                }

            }

            return entity;
        }

        #endregion

    }
}
