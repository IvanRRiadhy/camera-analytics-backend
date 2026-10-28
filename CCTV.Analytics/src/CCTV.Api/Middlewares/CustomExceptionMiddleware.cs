using System.Text.Json;
using CCTV.Contracts.Common;
using CCTV.Domain.Exceptions;
using CCTV.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CCTV.Api.Middlewares;

public class CustomExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomExceptionMiddleware> _logger;

    public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        object result;
        int statusCode;

        switch (exception)
        {
            case NotFoundException ex:
                statusCode = 404;
                result = ApiResponse.NotFound(ex.Message);
                _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
                break;

            case BusinessException ex:
                statusCode = 400;
                result = ApiResponse.BadRequest(ex.Message);
                _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
                break;

            case UnauthorizedException ex:
                statusCode = 401;
                result = ApiResponse.Unauthorized(ex.Message);
                _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
                break;

            case UnauthorizedAccessException or TenantNotResolvedException:
                statusCode = 403;
                result = ApiResponse.Forbidden(ResponseMessages.Forbidden);
                _logger.LogWarning(exception, "Forbidden access: {Message}", exception.Message);
                break;

            case FluentValidation.ValidationException ex:
                statusCode = 400;
                var validationErrors = ex.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                }).ToList();
                result = ApiResponse.BadRequest(ResponseMessages.ValidationFailed, validationErrors);
                _logger.LogWarning(ex, "Validation failure with {Count} errors.", ex.Errors.Count());
                break;

            case KeyNotFoundException or FileNotFoundException:
                statusCode = 404;
                result = ApiResponse.NotFound(exception.Message);
                _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                break;

            case ArgumentNullException ex:
                statusCode = 400;
                result = ApiResponse.BadRequest($"Parameter '{ex.ParamName}' is required.");
                _logger.LogWarning(ex, "Null argument: {Param}", ex.ParamName);
                break;

            case ArgumentException or InvalidOperationException:
                statusCode = 400;
                result = ApiResponse.BadRequest(exception.Message);
                _logger.LogWarning(exception, "Invalid operation/argument: {Message}", exception.Message);
                break;

            case DbUpdateException ex:
                statusCode = 400;
                var dbError = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
                result = ApiResponse.BadRequest(ResponseMessages.DatabaseError);
                _logger.LogError(ex, "Database error: {DbError}", dbError);
                break;

            case JsonException ex:
                statusCode = 400;
                result = ApiResponse.BadRequest(ResponseMessages.JsonError);
                _logger.LogWarning(ex, "JSON parsing error: {Message}", ex.Message);
                break;

            default:
                statusCode = 500;
                var safeMsg = !string.IsNullOrWhiteSpace(exception.Message) ? exception.Message : ResponseMessages.InternalServerError;
                result = ApiResponse.InternalError(safeMsg);
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        response.StatusCode = statusCode;

        await response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
