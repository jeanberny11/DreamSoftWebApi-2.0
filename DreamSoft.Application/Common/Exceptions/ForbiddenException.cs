namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when credentials are valid but access is blocked due to account or tenant state.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException(string errorCode, string message) : ApplicationException(message)
{
    /// <summary>
    /// Programmatic error code for the client to react to the specific blocked state.
    /// e.g. ACCOUNT_LOCKED, EMAIL_NOT_VERIFIED, TENANT_PENDING_EMAIL, etc.
    /// </summary>
    public string ErrorCode { get; } = errorCode;
}
