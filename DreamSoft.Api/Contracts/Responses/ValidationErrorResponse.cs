namespace DreamSoft.Api.Contracts.Responses;

/// <summary>
/// Validation error response with field-level error details
/// </summary>
public class ValidationErrorResponse
{
    /// <summary>
    /// HTTP status code (always 400 for validation errors)
    /// </summary>
    public int StatusCode { get; init; } = 400;

    /// <summary>
    /// Error code for programmatic handling
    /// </summary>
    public string ErrorCode { get; init; } = "VALIDATION_ERROR";

    /// <summary>
    /// User-friendly error type
    /// </summary>
    public string ErrorType { get; init; } = "Validation Error";

    /// <summary>
    /// Localized general validation error message
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
    /// Field-level validation errors
    /// Key: Field name, Value: Array of error messages for that field
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = new();
}
