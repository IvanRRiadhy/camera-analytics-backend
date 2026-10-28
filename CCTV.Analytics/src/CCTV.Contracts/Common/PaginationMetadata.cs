using System.Text.Json.Serialization;

namespace CCTV.Contracts.Common;

public class PaginationMetadata
{
    public PaginationMetadata() { }

    public PaginationMetadata(int page, int limit, int totalItems)
    {
        Page = page;
        Limit = limit;
        TotalItems = totalItems;
        TotalPages = limit > 0 ? (int)Math.Ceiling(totalItems / (double)limit) : 0;
        HasNextPage = page < TotalPages;
        HasPreviousPage = page > 1;
    }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; set; }

    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; set; }
}
