namespace CCTV.Contracts.Common;

public record ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public string Message { get; init; } = "Operation completed successfully.";
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Operation completed successfully.", Guid? correlationId = null) =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
            CorrelationId = correlationId ?? Guid.NewGuid()
        };
}

public record PagedResponse<T>
{
    public bool Success { get; init; } = true;
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public long TotalRecords { get; init; }
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
