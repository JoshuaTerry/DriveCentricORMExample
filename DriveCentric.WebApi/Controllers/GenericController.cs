using DriveCentric.Services;
using DriveCentric.Services.Interfaces;
using DriveCentric.Shared.Enums;
using DriveCentric.Shared.Interfaces;
using System;
using System.Web.Http;

namespace DriveCentric.WebApi.Controllers
{
    public abstract class GenericController<T> : ControllerBase<T> where T : class, IEntity
    {
        public GenericController(IService<T> serviceBase) : base(serviceBase) { }

        public virtual IHttpActionResult GetAll(int? limit = SearchParameters.LimitMax, int? offset = SearchParameters.OffsetDefault, string orderBy = "Id", string fields = null)
        {
            var search = new PageableSearch()
            {
                Limit = limit,
                Offset = offset,
                OrderBy = orderBy
            };

            return GetAll(search, fields);
        }

        public virtual IHttpActionResult GetAll(IPageable search, string fields = null)
        {
            try
            {
                fields = ConvertFieldList(fields, FieldsForList);
                return FinalizeResponse(Service.GetAll(fields, search), search, fields);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return InternalServerError(new Exception(ex.Message));
            }
        }
    }
}