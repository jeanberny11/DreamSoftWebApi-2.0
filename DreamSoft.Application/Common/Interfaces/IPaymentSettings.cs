namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Payment settings abstraction — keeps the Application layer unaware of any
/// specific payment provider configuration (e.g. Stripe, PayPal).
/// Implemented by provider-specific settings classes in Infrastructure.
/// </summary>
public interface IPaymentSettings
{
    /// <summary>
    /// Base URL of the frontend app, no trailing slash
    /// (e.g. http://localhost:5173, https://app.dreamsoft.com).
    /// Handlers compose per-request success/cancel URLs from it.
    /// </summary>
    string FrontendBaseUrl { get; }
}
