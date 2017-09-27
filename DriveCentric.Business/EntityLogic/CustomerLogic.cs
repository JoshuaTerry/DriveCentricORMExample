using DriveCentric.Shared.Models;

namespace DriveCentric.Business.EntityLogic
{
    public class CustomerLogic : EntityLogicBase<Customer>
    {
        //Customized Validation Logic to be called by the Service Base Implementation on Insert and Update
        public override void Validate(Customer entity)
        {
            base.Validate(entity);
        }
    }
}
