using DreamSoft.Application.Common;

namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends email verification code to user.
    /// Returns <see cref="EmailSendResult"/> so callers can inspect the error
    /// message when delivery fails without catching exceptions.
    /// </summary>
    Task<EmailSendResult> SendVerificationCodeAsync(
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

    /// <summary>
    /// Sends subscription confirmation email with full invoice detail after
    /// a successful first payment. Triggered by the invoice.paid webhook.
    /// </summary>
    Task SendSubscriptionConfirmationAsync(
        string toEmail,
        string firstName,
        string companyName,
        string subdomain,
        string planName,
        string billingCycle,
        decimal amount,
        string currency,
        string invoiceNumber,
        DateTime paidAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends payment failed notification when a first-time checkout is
    /// abandoned/expired or a card is declined. Includes instructions to retry.
    /// </summary>
    Task SendPaymentFailedAsync(
        string toEmail,
        string firstName,
        string companyName,
        string planName,
        string? failureReason,
        CancellationToken cancellationToken = default);
}
