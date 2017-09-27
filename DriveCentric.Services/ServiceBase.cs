using DriveCentric.Business.Helpers;
using DriveCentric.Business.Interfaces;
using DriveCentric.Data;
using DriveCentric.Logger;
using DriveCentric.Services.Interfaces;
using DriveCentric.Shared;
using DriveCentric.Shared.Extensions;
using DriveCentric.Shared.Helpers;
using DriveCentric.Shared.Interfaces;
using DriveCentric.Shared.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DriveCentric.Services
{
    public class ServiceBase<T> : IService<T> where T : class, IEntity
    {
        private readonly ILogger logger = LoggerManager.GetLogger(typeof(ServiceBase<T>));
        private readonly IUnitOfWork unitOfWork;
        private Expression<Func<T, object>>[] includesForSingle = null;
        private Expression<Func<T, object>>[] includesForList = null;

        protected ILogger Logger => logger;

        /// <summary>
        /// Formatting and other logic for an entity retrieved for a GET.
        /// </summary>
        protected virtual Action<T, string> FormatEntityForGet => DefaultFormatEntityForGet;

        public ServiceBase() : this(new UnitOfWorkEF()) { }
        public ServiceBase(IUnitOfWork uow)
        {
            unitOfWork = uow;
        }

        public IUnitOfWork UnitOfWork
        {
            get { return unitOfWork; }
        }

        public virtual Expression<Func<T, object>>[] IncludesForSingle
        {
            protected get { return includesForSingle; }
            set { includesForSingle = value; }
        }

        public virtual Expression<Func<T, object>>[] IncludesForList
        {
            protected get { return includesForList; }
            set { includesForList = value; }
        }

        public virtual IDataResponse<IEnumerable<ICanTransmogrify>> GetAll()
        {
            return GetAll(null, null);
        }

        public virtual IDataResponse<IEnumerable<ICanTransmogrify>> GetAll(string fields, IPageable search = null)
        {
            var queryable = unitOfWork.GetEntities(includesForList);
            return GetPagedResults(queryable, search, fields);
        }

        protected IDataResponse<IEnumerable<ICanTransmogrify>> GetPagedResults(IQueryable<T> queryable, IPageable search = null, string fields = null)
        {
            if (search == null)
            {
                search = PageableSearch.Default;
            }

            var query = new CriteriaQuery<T, IPageable>(queryable, search);

            if (!string.IsNullOrWhiteSpace(search.OrderBy))
            {
                query = query.SetOrderBy(search.OrderBy);
            }

            var totalCount = query.GetQueryable().Count();

            query = query.SetLimit(search.Limit)
                         .SetOffset(search.Offset);

            //var sql = query.GetQueryable().ToString();  //This shows the SQL that is generated
            var queryData = query.GetQueryable().AsEnumerable(); // AsEnumerable() runs the SQL query.
            
            var queryDataList = queryData.ToList();          

            FormatEntityListForGet(queryDataList, fields);

            var response = GetIDataResponse(() => queryDataList.AsEnumerable<ICanTransmogrify>());

            response.TotalResults = totalCount;

            return response;
        }

      

        /// <summary>
        /// Set a DateTime? property to UTC if the property contains a DateTime value with a non-zero time of day.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="path">Path to DateTime? property.</param>
        /// <returns>The service (to allow chaining of method calls.)</returns>
        protected ServiceBase<T> SetDateTimeKind(T entity, Expression<Func<T, DateTime?>> path)
        {
            if (entity != null && path != null)
            {
                var expr = (MemberExpression)path.Body;
                var prop = (PropertyInfo)expr.Member;
                DateTime? dt = (DateTime?)prop.GetValue(entity);

                prop.SetValue(entity, DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc));
            }
            return this;
        }

        /// <summary>
        /// Set a DateTime property to UTC if the property contains a DateTime value with a non-zero time of day.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="path">Path to DateTime property.</param>
        /// <returns>The service (to allow chaining of method calls.)</returns>
        protected ServiceBase<T> SetDateTimeKind(T entity, Expression<Func<T, DateTime>> path)
        {
            if (entity != null && path != null)
            {
                var expr = (MemberExpression)path.Body;
                var prop = (PropertyInfo)expr.Member;
                DateTime dt = (DateTime)prop.GetValue(entity);

                prop.SetValue(entity, DateTime.SpecifyKind(dt, DateTimeKind.Utc));
            }
            return this;
        }

        /// <summary>
        /// Provides a virtual method that can be overridden to modify the sort order of the results of a GET.
        /// </summary>
        /// <param name="orderBy"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual List<T> ModifySortOrder(string orderBy, List<T> data)
        {
            return data;
        }

        /// <summary>
        /// Determine if every field in a field list can be mapped to a property in the specified type.
        /// </summary>
        protected bool VerifyFieldList<T1>(string fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return false;
            }

            var properties = typeof(T1).GetProperties().Select(p => p.Name.ToUpper());
            return fields.ToUpper().Split(',').All(f => properties.Contains(f));
        }

        /// <summary>
        /// Determine if a field list contains any of the specified properties.<para/>
        /// Returns true if the field list is "all".<para/>
        /// Returns <false if the field list is null or empty.
        /// </summary>
        internal bool FieldListHasProperties(string fields, params Expression<Func<T, object>>[] properties)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return false;
            }
            else if (string.Compare(fields, FieldLists.AllFields, true) == 0)
            {
                return true;
            }

            var fieldsUpper = fields.ToUpper().Split(',');

            return properties.Any(p => fieldsUpper.Contains(PathHelper.NameFor<T>(p).ToUpper()));
        }

        /// <summary>
        /// Determine if a field list contains any of the specified properties.<para/>
        /// Returns true if the field list is "all".<para/>
        /// Returns false if the field list is null or empty.
        /// </summary>
        internal bool FieldListHasProperties(string fields, params string[] properties)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return false;
            }
            else if (string.Compare(fields, FieldLists.AllFields, true) == 0)
            {
                return true;
            }

            var fieldsUpper = fields.ToUpper().Split(',');

            return properties.Any(p => fieldsUpper.Contains(p.ToUpper()));
        }

        public virtual IDataResponse<T> GetById(Guid id)
        {
            T result = unitOfWork.GetById(id, includesForSingle);
            
            FormatEntityForGet(result, FieldLists.AllFields);
            return GetIDataResponse(() => result);
        }

        public IDataResponse<T> GetWhereExpression(Expression<Func<T, bool>> expression)
        {
            IDataResponse<T> response = GetIDataResponse(() => UnitOfWork.GetRepository<T>().GetEntities(includesForSingle).Where(expression).FirstOrDefault());
            FormatEntityForGet(response.Data, FieldLists.AllFields);
            return response;
        }

        public IDataResponse<IEnumerable<ICanTransmogrify>> GetAllWhereExpression(Expression<Func<T, bool>> expression, IPageable search = null, string fields = null)
        {
            var queryable = UnitOfWork.GetEntities(includesForList).Where(expression);
            return GetPagedResults(queryable, search, fields);
        }

        /// <summary>
        /// Formatting and other logic for a single entity retrieved for a GET.
        /// </summary>
        private void DefaultFormatEntityForGet(T entity, string fields) { }

        /// <summary>
        /// Formatting and other logic for a list of entities retrieved for a GET.
        /// </summary>
        protected void FormatEntityListForGet(IList<T> list, string fields)
        {
            if (FormatEntityForGet != DefaultFormatEntityForGet && FormatEntityForGet != null) // If overridden
            {
                if (fields == null)
                {
                    fields = string.Empty;
                }
                list.ForEach(p => FormatEntityForGet(p, fields));
            }
        }

        public virtual IDataResponse Update(T entity)
        {
            var response = new DataResponse<T>();
            try
            {
                BusinessLogicHelper.GetBusinessLogic<T>(unitOfWork).Validate(entity);
                unitOfWork.Update(entity);
                unitOfWork.SaveChanges();
                response.Data = unitOfWork.GetById(entity.Id, IncludesForSingle);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return ProcessIDataResponseException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update an entity from a JObject (during a PATCH) then commit the changes to the database.
        /// </summary>
        /// <param name="id">Entity Id</param>
        /// <param name="changes">Changes as a JObject.</param>
        public virtual IDataResponse<T> Update(Guid id, JObject changes)
        {
            return Update(unitOfWork.GetById<T>(id, IncludesForSingle), changes);
        }

        /// <summary>
        /// Update an entity from a JObject (during a PATCH) then commit the changes to the database.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="changes">Changes as a JObject.</param>
        public virtual IDataResponse<T> Update(T entity, JObject changes)
        {
            var response = new DataResponse<T>();

            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                UpdateFromJObject(entity, changes);

                Guid id = entity.Id;

                unitOfWork.SaveChanges();

                response.Data = unitOfWork.GetById(id, IncludesForSingle);
                FormatEntityForGet(response.Data, FieldLists.AllFields);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return ProcessIDataResponseException(ex);
            }

            return response;
        }


        /// <summary>
        /// Allows custom processing of a JToken during the Update method (during a PATCH).
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="token">Property value, as a JToken</param>
        /// <returns>TRUE if the property was updated by the method override.</returns>
        protected virtual bool ProcessJTokenUpdate(IEntity entity, string name, JToken token) => false;


        /// <summary>
        /// Update an entity's properties from a JObject (during a PATCH).  Validation is performed, but no changes are saved.
        /// </summary>
        /// <param name="entity">Entity to be updated.</param>
        /// <param name="changes">Changes as a JObject.</param>
        protected virtual void UpdateFromJObject<T1>(T1 entity, JObject changes) where T1 : class, IEntity
        {
            Dictionary<string, object> changedProperties = new Dictionary<string, object>();
            Type entityType = typeof(T1);

            foreach (var pair in changes)
            {
                if (!ProcessJTokenUpdate(entity, pair.Key, pair.Value))
                {
                    var convertedPair = JsonExtensions.ConvertToType<T1>(pair);
                    changedProperties.Add(convertedPair.Key, convertedPair.Value);
                }
            }

            IEntityLogic logic = BusinessLogicHelper.GetBusinessLogic<T1>(unitOfWork);

            unitOfWork.GetRepository<T1>().UpdateChangedProperties(entity, changedProperties, p => logic.Validate(p));
        }

        /// <summary>
        /// Add or update entities from a JArray of changes during a PATCH.
        /// </summary>
        /// <param name="entityCollection">Collection of entities:  New entities will be added to this collection.</param>
        /// <param name="changeArray">JArray of changes.</param>
        protected void AddUpdateFromJArray<T1>(ICollection<T1> entityCollection, JArray changeArray) where T1 : class, IEntity
        {
            if (changeArray == null)
            {
                return;
            }

            foreach (JObject jobject in changeArray)
            {
                T1 entity = jobject.ToObject<T1>();
                if (entity != null)
                {
                    T1 foundEntity = null;
                    if (entity.Id != Guid.Empty)
                    {
                        foundEntity = unitOfWork.GetById<T1>(entity.Id);
                    }
                    if (foundEntity != null)
                    {
                        // The entity already exists.
                        UpdateFromJObject(foundEntity, jobject);
                    }
                    else
                    {
                        // The entity doesn't exist - either the ID was empty, or it's a new ID.
                        unitOfWork.Insert(entity);
                        entityCollection?.Add(entity);
                        BusinessLogicHelper.GetBusinessLogic<T1>(unitOfWork)?.Validate(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Add or update an entity from a JOBject during a PATCH.  The new or updated entity is returned.
        /// </summary>
        /// <param name="changeObject">JObject containing changes.</param>
        /// <returns></returns>
        protected T1 AddUpdateFromJObject<T1>(JObject changeObject) where T1 : class, IEntity
        {
            T1 entity = changeObject.ToObject<T1>();
            if (entity != null)
            {
                T1 foundEntity = null;
                if (entity.Id != Guid.Empty)
                {
                    foundEntity = unitOfWork.GetById<T1>(entity.Id);
                }
                if (foundEntity != null)
                {
                    // The entity already exists.
                    UpdateFromJObject(foundEntity, changeObject);
                    entity = foundEntity;
                }
                else
                {
                    // The entity doesn't exist - either the ID was empty, or it's a new ID.
                    unitOfWork.Insert(entity);
                    BusinessLogicHelper.GetBusinessLogic<T1>(unitOfWork)?.Validate(entity);
                }
            }
            return entity;
        }

        public virtual IDataResponse<T> Add(T entity)
        {
            var response = new DataResponse<T>();
            try
            {
                unitOfWork.Insert(entity);
                BusinessLogicHelper.GetBusinessLogic<T>(unitOfWork)?.Validate(entity);
                unitOfWork.SaveChanges();
                response.Data = unitOfWork.GetById(entity.Id, IncludesForSingle);
                FormatEntityForGet(response.Data, FieldLists.AllFields);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return ProcessIDataResponseException(ex);
            }

            return response;
        }

        public virtual IDataResponse Delete(T entity)
        {
            var response = new DataResponse();
            try
            {
                unitOfWork.Delete(entity);
                unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return ProcessIDataResponseException(ex);
            }

            return response;
        }

        public IDataResponse<T1> GetIDataResponse<T1>(Func<T1> funcToExecute, string fieldList = null, bool shouldAddLinks = false)
        {
            return GetDataResponse(funcToExecute, fieldList, shouldAddLinks);
        }

        public DataResponse<T1> GetDataResponse<T1>(Func<T1> funcToExecute, string fieldList = null, bool shouldAddLinks = false)
        {
            try
            {
                var result = funcToExecute();
                var response = new DataResponse<T1>
                {
                    Data = result,
                    IsSuccessful = true
                };
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return ProcessDataResponseException<T1>(ex);
            }
        }

        public IDataResponse GetDataResponse(Action actionToExecute)
        {
            DataResponse response;

            try
            {
                actionToExecute();
                response = new DataResponse
                {
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return ProcessIDataResponseException(ex);
            }

            return response;
        }

        public IDataResponse GetErrorResponse(string errorMessage, string verboseErrorMessage = null)
        {
            return GetErrorResponse<object>(errorMessage, verboseErrorMessage);
        }

        public IDataResponse<T1> GetErrorResponse<T1>(string errorMessage, string verboseErrorMessage = null)
        {
            Logger.LogError($"Message: {errorMessage} | Verbose Message: {verboseErrorMessage}");

            return (verboseErrorMessage == null)
                ? GetErrorResponse<T1>(new List<string> { errorMessage })
                : GetErrorResponse<T1>(new List<string> { errorMessage }, new List<string> { verboseErrorMessage });
        }

        public IDataResponse<T1> GetErrorResponse<T1>(IEnumerable<string> errorMessages, IEnumerable<string> verboseErrorMessages = null)
        {
            return new DataResponse<T1>
            {
                IsSuccessful = false,
                Data = default(T1),
                ErrorMessages = errorMessages?.ToList(),
                VerboseErrorMessages = verboseErrorMessages?.ToList()
            };
        }

        public IDataResponse<T> ProcessIDataResponseException(Exception ex)
        {
            var response = new DataResponse<T>();
            response.IsSuccessful = false;
            response.ErrorMessages.Add(ex.Message);
            response.VerboseErrorMessages.Add(ex.ToString());
            Logger.LogError(ex.ToString());

            return response;
        }

        public DataResponse<T1> ProcessDataResponseException<T1>(Exception ex)
        {
            var response = new DataResponse<T1>();
            response.IsSuccessful = false;
            response.ErrorMessages.Add(ex.Message);
            response.VerboseErrorMessages.Add(ex.ToString());
            Logger.LogError(ex.ToString());

            return response;

        }
    }
}
