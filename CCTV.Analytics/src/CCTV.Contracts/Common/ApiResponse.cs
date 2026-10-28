using System.Text.Json.Serialization;

namespace CCTV.Contracts.Common;

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("msg")]
    public string Msg { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    [JsonPropertyOrder(10)]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMetadata? Meta { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string msg = "Success", int code = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Msg = msg,
            Code = code,
            Data = data,
            Meta = null
        };
    }

    public static ApiResponse<List<TItem>> PaginatedResponse<TItem>(PagedResult<TItem> pagedResult, string msg = "Success", int code = 200)
    {
        return new ApiResponse<List<TItem>>
        {
            Success = true,
            Msg = msg,
            Code = code,
            Data = pagedResult.Data,
            Meta = new PaginationMetadata
            {
                Page = pagedResult.Page,
                Limit = pagedResult.Limit,
                TotalItems = pagedResult.TotalItems,
                TotalPages = pagedResult.TotalPages,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage
            }
        };
    }

    public static ApiResponse<object> SuccessResponse(string msg = "Success", int code = 200)
    {
        return new ApiResponse<object>
        {
            Success = true,
            Msg = msg,
            Code = code,
            Data = null,
            Meta = null
        };
    }

    public static ApiResponse<object> ErrorResponse(string msg, int code = 400, object? data = null)
    {
        return new ApiResponse<object>
        {
            Success = false,
            Msg = msg,
            Code = code,
            Data = data,
            Meta = null
        };
    }

    public static ApiResponse<object> NotFound(string message) => ErrorResponse(message, 404);

    public static ApiResponse<object> BadRequest(string message) => ErrorResponse(message, 400);

    public static ApiResponse<object> BadRequest(string message, object errors) => ErrorResponse(message, 400, errors);

    public static ApiResponse<object> Unauthorized(string message) => ErrorResponse(message, 401);

    public static ApiResponse<object> Forbidden(string message) => ErrorResponse(message, 403);

    public static ApiResponse<object> InternalError(string message) => ErrorResponse(message, 500);
}

public class ApiResponse : ApiResponse<object>
{
}
