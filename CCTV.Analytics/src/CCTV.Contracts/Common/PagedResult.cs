namespace CCTV.Contracts.Common;

public class PagedResult<T>
{
    public List<T> Data { get; set; } = [];
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => Limit > 0 ? (int)Math.Ceiling((double)TotalItems / Limit) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PagedResult() { }

    public PagedResult(List<T> data, int totalItems, int page, int limit)
    {
        Data = data;
        TotalItems = totalItems;
        Page = page;
        Limit = limit;
    }
}
