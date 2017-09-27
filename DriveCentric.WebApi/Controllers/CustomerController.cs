using DriveCentric.Services;
using DriveCentric.Services.Interfaces;
using DriveCentric.Shared.Enums;
using DriveCentric.Shared.Models;
using System.Web.Http;

namespace DriveCentric.WebApi.Controllers
{
    public class CustomerController : GenericController<Customer>
    {
        public CustomerController(IService<Customer> service) : base(service) { }
        public CustomerController() : this(new ServiceBase<Customer>()) { }

        [HttpGet]
        [Route("api/v1/customers")]
        public override IHttpActionResult GetAll(int? limit = SearchParameters.LimitMax, int? offset = SearchParameters.OffsetDefault, string orderBy = null, string fields = null)
        {
            return base.GetAll(limit, offset, orderBy, fields);
        }

    }
}