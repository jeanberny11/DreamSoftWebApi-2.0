namespace DreamSoft.Application.Common.Models;

/// <summary>
/// Standard error response for all API errors
/// Used across all features (Authentication, Inventory, Sales, etc.)
/// </summary>
public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string ErrorCode { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
    public string? ErrorType { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates error response for validation failures
    /// </summary>
    public static ErrorResponse ValidationError(Dictionary<string, string[]> validationErrors)
    {
        return new ErrorResponse
        {
            Success = false,
            ErrorCode = "VALIDATION_ERROR",
            ErrorMessage = "One or more validation errors occurred",
            ErrorType = "Validation",
            ValidationErrors = validationErrors,
            StatusCode = 400,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates error response for unauthorized access
    /// </summary>
    public static ErrorResponse UnauthorizedError(string message = "Unauthorized access")
    {
        return new ErrorResponse
        {
            Success = false,
            ErrorCode = "UNAUTHORIZED",
            ErrorMessage = message,
            ErrorType = "Authorization",
            StatusCode = 401,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates error response for not found resources
    /// </summary>
    public static ErrorResponse NotFoundError(string message)
    {
        return new ErrorResponse
        {
            Success = false,
            ErrorCode = "NOT_FOUND",
            ErrorMessage = message,
            ErrorType = "NotFound",
            StatusCode = 404,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates error response for conflicts (duplicate data)
    /// </summary>
    public static ErrorResponse ConflictError(string message)
    {
        return new ErrorResponse
        {
            Success = false,
            ErrorCode = "CONFLICT",
            ErrorMessage = message,
            ErrorType = "Conflict",
            StatusCode = 409,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates error response for rate limit exceeded
    /// </summary>
    public static ErrorResponse RateLimitError(string message = "Too many requests")
    {
        return new ErrorResponse
        {
            Success = false,
            ErrorCode = "RATE_LIMIT_EXCEEDED",
            ErrorMessage = message,
            ErrorType = "RateLimit",
            StatusCode = 429,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates error response for server errors
    /// </summary>
    public static ErrorResponse ServerError(string message = "An internal server error occurred")
    {
        return new ErrorResponse
        {
            Success = false,
            ErrorCode = "INTERNAL_SERVER_ERROR",
            ErrorMessage = message,
            ErrorType = "ServerError",
            StatusCode = 500,
            Timestamp = DateTime.UtcNow
        };
    }
}