namespace DriveCentric.Shared.Interfaces
{
    public interface IPageable
    {
        int? Offset { get; set; }
        int? Limit { get; set; }
        string OrderBy { get; set; }
    }
}
