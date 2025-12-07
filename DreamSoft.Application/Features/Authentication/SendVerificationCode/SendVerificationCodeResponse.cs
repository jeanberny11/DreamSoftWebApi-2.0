namespace DreamSoft.Application.Features.Authentication.SendVerificationCode;

/// <summary>
/// Response after sending verification code
/// </summary>
public class SendVerificationCodeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public int ExpiresInSeconds { get; set; } // 300 seconds = 5 minutes
}