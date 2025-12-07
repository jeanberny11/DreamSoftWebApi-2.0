namespace DreamSoft.Application.Features.Authentication.VerifyCode;

/// <summary>
/// Response after verifying email code
/// </summary>
public class VerifyCodeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string SessionToken { get; set; } = null!; // JWT token for Step 2
    public int ExpiresInSeconds { get; set; } // 1800 seconds = 30 minutes
}