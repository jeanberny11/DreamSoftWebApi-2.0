namespace DreamSoft.Api.Contracts.Responses;

/// <summary>
/// Standard error response for all non-validation exceptions
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error code for programmatic handling (e.g., "USER_NOT_FOUND")
    /// </summary>
    public string ErrorCode { get; set; } = null!;

    /// <summary>
    /// User-friendly error type (e.g., "Not Found", "Unauthorized")
    /// </summary>
    public string ErrorType { get; set; } = null!;

    /// <summary>
    /// Localized error message
    /// </summary>
    public string ErrorMessage { get; set; } = null!;

    /// <summary>
    /// Trace identifier for correlation with logs
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Stack trace (only included in Development environment)
    /// </summary>
    public string? StackTrace { get; set; }
}
