namespace DreamSoft.Api.Contracts.Responses;

/// <summary>
/// Standard error codes for programmatic error handling.
/// Auth forbidden codes mirror ForbiddenErrorCodes in the Application layer.
/// </summary>
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound        = "NOT_FOUND";
    public const string Unauthorized    = "UNAUTHORIZED";
    public const string Forbidden       = "FORBIDDEN";
    public const string Conflict        = "CONFLICT";
    public const string InternalError   = "INTERNAL_ERROR";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string EmailSendFailed   = "EMAIL_SEND_FAILED";
}
