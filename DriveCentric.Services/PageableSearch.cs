using DriveCentric.Shared.Enums;
using DriveCentric.Shared.Interfaces;

namespace DriveCentric.Services
{
    public class PageableSearch : IPageable
    {
        public static PageableSearch Default => new PageableSearch(SearchParameters.OffsetDefault, SearchParameters.LimitDefault, null);
        public static PageableSearch Max => new PageableSearch(SearchParameters.OffsetDefault, SearchParameters.LimitMax, null);

        #region IPageable
        public int? Offset { get; set; }
        public int? Limit { get; set; }
        public string OrderBy { get; set; }

        public PageableSearch()
        {
        }

        public PageableSearch(int? offset, int? limit, string orderBy)
        {
            Offset = offset;
            Limit = limit;
            OrderBy = orderBy;
        }

        #endregion
    }
}
