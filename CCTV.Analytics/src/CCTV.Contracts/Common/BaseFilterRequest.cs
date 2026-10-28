namespace CCTV.Contracts.Common;

public class BaseFilterRequest
{
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
    public int? Limit { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; } = false;
}
