namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends email verification code to user
    /// </summary>
    Task<bool> SendVerificationCodeAsync(
        string toEmail,
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends welcome email after successful registration
    /// </summary>
    Task SendWelcomeEmailAsync(
        string toEmail,
        string firstName,
        string companyName,
        string subdomain,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends password reset email
    /// </summary>
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetToken,
        CancellationToken cancellationToken = default);
}