using DriveCentric.Business.Interfaces;
using DriveCentric.Data;
using DriveCentric.Shared.Interfaces;
using System;
using System.Linq;

namespace DriveCentric.Business
{
    public class EntityLogicBase<T> : EntityLogicBase where T : class, IEntity
    { 

        #region Constructors 

        public EntityLogicBase() : this(new UnitOfWorkEF()) { }

        public EntityLogicBase(IUnitOfWork uow) : base(uow)
        {
            
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Validate an entity.
        /// </summary>
        public virtual void Validate(T entity) { }
         
        /// <summary>
        /// Validate an entity.
        /// </summary>
        public override void Validate(IEntity entity)
        {
            T typedEntity = entity as T;
            if (typedEntity != null)
            {
                Validate(typedEntity);
            }
        }
         

        /// <summary>
        /// Validate a set of strings, ensuring they are non-blank and unique.
        /// </summary>
        /// <param name="numberOfStrings">If non-zero, validate only the first n strings.</param>
        /// <param name="errorMessageParameter">Paramter to include in the error message (i.e. what the strings represent.)</param>
        /// <param name="strings">List of strings to be validated.</param>
        protected void ValidateNonBlankAndUnique(int numberOfStrings, string errorMessageParameter, params string[] strings)
        {
            if (strings.Length == 0)
            {
                return;
            }

            if (numberOfStrings == 0)
            {
                numberOfStrings = strings.Length;
            }

            var stringsToUpper = strings.Take(numberOfStrings).Select(p => p.ToUpper());

            if (stringsToUpper.Any(p => string.IsNullOrWhiteSpace(p)))
            {
                throw new Exception("Must be non-blank");
            }
            if (stringsToUpper.Distinct().Count() < numberOfStrings)
            {
                throw new Exception("Must be non-unique");
            }
        }
        #endregion

    }

    /// <summary>
    /// Non-generic, non-strongly-typed base class for entity business logic.
    /// </summary>
    public class EntityLogicBase : IEntityLogic, IDisposable
    {
        /// <summary>
        /// Unit of work used by the business logic.
        /// </summary>
        public IUnitOfWork UnitOfWork { get; private set; }

        public EntityLogicBase(IUnitOfWork uow)
        {
            this.UnitOfWork = uow;
            uow.AddBusinessLogic(this);
        }

        /// <summary>
        /// Validate an entity.
        /// </summary>
        public virtual void Validate(IEntity entity) { }

        /// <summary>
        /// Update an entity in Elasticsearch by building the search document and indexing it.
        /// </summary>
         

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UnitOfWork?.Dispose();
                }

                UnitOfWork = null;

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion

    }
}
