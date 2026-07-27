namespace CCTV.Contracts.Common;

public record ApiErrorResponse
{
    public bool Success { get; init; } = false;
    public string ErrorCode { get; init; } = "INTERNAL_SERVER_ERROR";
    public string Message { get; init; } = "An unexpected error occurred.";
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public IReadOnlyList<ErrorDetail> Errors { get; init; } = Array.Empty<ErrorDetail>();

    public static ApiErrorResponse Fail(string errorCode, string message, IEnumerable<ErrorDetail>? details = null, Guid? correlationId = null) =>
        new()
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            Errors = details?.ToList() ?? new List<ErrorDetail>(),
            CorrelationId = correlationId ?? Guid.NewGuid()
        };
}

public record ErrorDetail(string Field, string Detail);
