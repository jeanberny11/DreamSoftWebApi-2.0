namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Payment settings abstraction — keeps the Application layer unaware of any
/// specific payment provider configuration (e.g. Stripe, PayPal).
/// Implemented by provider-specific settings classes in Infrastructure.
/// </summary>
public interface IPaymentSettings
{
    /// <summary>Frontend URL redirected to after a successful payment.</summary>
    string SuccessUrl { get; }

    /// <summary>Frontend URL redirected to when the user cancels checkout.</summary>
    string CancelUrl { get; }
}
