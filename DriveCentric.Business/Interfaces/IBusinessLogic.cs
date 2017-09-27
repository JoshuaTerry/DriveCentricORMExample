using DriveCentric.Shared.Interfaces;

namespace DriveCentric.Business.Interfaces
{
    public interface IBusinessLogic
    {
        IUnitOfWork UnitOfWork { get; }
    }
}
