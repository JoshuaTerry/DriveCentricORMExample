using DriveCentric.Shared.Interfaces;

namespace DriveCentric.Business.Interfaces
{
    public interface IEntityLogic : IBusinessLogic
    {
        /// <summary>
        /// Validate an entity.
        /// </summary>
        void Validate(IEntity entity);
         
    }
}
