using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.ViewModels.Api
{
    // Query string for list endpoints: ?page=2&pageSize=20
    public class PageQuery
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        // Capped at 50 so one request can't load the whole table.
        [Range(1, 50)]
        public int PageSize { get; set; } = 10;
    }

    // One page of results plus what the client needs to show page links.
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
