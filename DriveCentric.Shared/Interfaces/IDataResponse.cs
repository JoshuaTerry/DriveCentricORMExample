using System.Collections.Generic;

namespace DriveCentric.Shared.Interfaces
{
    public interface IDataResponse
    {
        int? TotalResults { get; set; }
        bool IsSuccessful { get; set; }
        List<string> ErrorMessages { get; set; }
        List<string> VerboseErrorMessages { get; set; }
    }

    public interface IDataResponse<T> : IDataResponse
    {
        T Data { get; set; }
    }
}
