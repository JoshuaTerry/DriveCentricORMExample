
using DriveCentric.Logger;
using DriveCentric.Services;
using DriveCentric.Services.Interfaces;
using DriveCentric.Shared;
using DriveCentric.Shared.Helpers;
using DriveCentric.Shared.Interfaces;
using DriveCentric.WebApi.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Http;

namespace DriveCentric.WebApi.Controllers
{
    public abstract class ControllerBase<T> : ApiController where T : class, IEntity
    {
        private readonly DynamicTransmogrifier dynamicTransmogrifier;
        private readonly IService<T> service;
        private readonly ILogger logger = LoggerManager.GetLogger(typeof(ControllerBase<T>));
        private PathHelper.FieldListBuilder<T> fieldListBuilder = null;

        internal IService<T> Service => service;
        protected ILogger Logger => logger;
        protected DynamicTransmogrifier DynamicTransmogrifier => dynamicTransmogrifier;
        protected PathHelper.FieldListBuilder<T> FieldListBuilder => fieldListBuilder?.Clear() ?? (fieldListBuilder = new PathHelper.FieldListBuilder<T>());

        protected virtual string FieldsForList => string.Empty;
        protected virtual string FieldsForSingle => FieldLists.AllFields;
        protected virtual string FieldsForAll => string.Empty;

        #region Constructors 

        public ControllerBase(IService<T> serviceBase)
        {
            dynamicTransmogrifier = new DynamicTransmogrifier();
            service = serviceBase;
            service.IncludesForSingle = GetDataIncludesForSingle();
            service.IncludesForList = GetDataIncludesForList();
        }

        #endregion

        #region Methods 

        protected virtual Expression<Func<T, object>>[] GetDataIncludesForSingle()
        {
            //Each controller should implement this if they need specific children populated
            return null;
        }

        protected virtual Expression<Func<T, object>>[] GetDataIncludesForList()
        {
            //Each controller should implement this if they need specific children populated
            return null;
        }

        /// <summary>
        /// Convert a comma delimited list of fields for GET.  "all" specifies all fields, blank or null specifies default fields.
        /// </summary>
        /// <param name="fields">List of fields from API call.</param>
        /// <param name="defaultFields">Default fields list.</param>
        protected virtual string ConvertFieldList(string fields, string defaultFields = "")
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                fields = defaultFields;
            }
            if (string.Compare(fields, FieldLists.AllFields, true) == 0)
            {
                fields = FieldsForAll;
            }

            return fields;
        }

        public virtual IHttpActionResult GetById(Guid id, string fields = null)
        {
            try
            {
                var response = service.GetById(id);
                if (!response.IsSuccessful)
                {
                    throw new Exception(string.Join(", ", response.ErrorMessages));
                }
                return FinalizeResponse(response, ConvertFieldList(fields, FieldsForSingle));
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                return InternalServerError(new Exception(ex.Message));
            }
        }

        protected IHttpActionResult FinalizeResponse<T1>(IDataResponse<IEnumerable<T1>> response, IPageable search, string fields = null)
            where T1 : class
        {
            try
            {
                if (search == null)
                {
                    search = PageableSearch.Default;
                }

                if (!response.IsSuccessful)
                {
                    return BadRequest(string.Join(", ", response.ErrorMessages));
                }

                var totalCount = response.TotalResults;

                var dynamicResponse = dynamicTransmogrifier.ToDynamicResponse(response, fields);
                if (!dynamicResponse.IsSuccessful)
                {
                    throw new Exception(string.Join(", ", dynamicResponse.ErrorMessages));
                }
                return Ok(dynamicResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                return InternalServerError(new Exception(ex.Message));
            }
        }

        protected IHttpActionResult FinalizeResponse(IDataResponse<T> response, string fields = null)
        {
            try
            {
                if (response.Data == null)
                {
                    if (response.ErrorMessages.Count > 0)
                        return BadRequest(string.Join(",", response.ErrorMessages));
                    else
                        return NotFound();
                }
                if (!response.IsSuccessful)
                {
                    return BadRequest(string.Join(",", response.ErrorMessages));
                }

                var dynamicResponse = dynamicTransmogrifier.ToDynamicResponse(response, fields);
                if (!dynamicResponse.IsSuccessful)
                {
                    throw new Exception(string.Join(", ", dynamicResponse.ErrorMessages));
                }

                return Ok(dynamicResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                return InternalServerError(new Exception(ex.Message));
            }
        }

        public virtual IHttpActionResult Post(T entity)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception("Model Invalid");
                }


                var response = service.Add(entity);

                if (!response.IsSuccessful)
                    throw new Exception(string.Join(",", response.ErrorMessages));

                return FinalizeResponse(response, string.Empty);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                return InternalServerError(new Exception(ex.Message));
            }
        }

        public virtual IHttpActionResult Patch(Guid id, JObject changes)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = service.Update(id, changes);

                if (!response.IsSuccessful)
                    throw new Exception(string.Join(",", response.ErrorMessages));

                return FinalizeResponse(response, string.Empty);

            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                return InternalServerError(new Exception(ex.Message));
            }
        }

        public virtual IHttpActionResult Delete(Guid id)
        {
            try
            {
                var entity = service.GetById(id);

                if (entity.Data == null)
                {
                    return NotFound();
                }

                if (!entity.IsSuccessful)
                {
                    return BadRequest(string.Join(",", entity.ErrorMessages));
                }

                var response = service.Delete(entity.Data);
                if (!response.IsSuccessful)
                {
                    return BadRequest(string.Join(", ", response.ErrorMessages));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                return InternalServerError(new Exception(ex.Message));
            }
        }

        /// <summary>
        /// Invoke a custom action that returns an IDataReponse
        /// </summary>
        /// <param name="action">Action to be invoked.</param>
        protected virtual IHttpActionResult CustomAction<T1>(Func<IDataResponse<T1>> action)
        {
            try
            {
                IDataResponse<T1> result = action();
                if (result.Data == null)
                {
                    return NotFound();
                }

                if (!result.IsSuccessful)
                {
                    return BadRequest(string.Join(",", result.ErrorMessages));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                return InternalServerError(new Exception(ex.Message));
            }
        }
        #endregion
    }
}