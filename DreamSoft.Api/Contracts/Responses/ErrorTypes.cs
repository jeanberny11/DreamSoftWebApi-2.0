namespace DreamSoft.Api.Contracts.Responses;

/// <summary>
/// User-friendly error type names
/// </summary>
public static class ErrorTypes
{
    public const string ValidationError = "Validation Error";
    public const string NotFound = "Not Found";
    public const string Unauthorized = "Unauthorized";
    public const string Forbidden = "Forbidden";
    public const string Conflict = "Conflict";
    public const string InternalError = "Internal Server Error";
    public const string RateLimitExceeded = "Rate Limit Exceeded";
    public const string EmailSendFailed = "Email Send Failed";
}
