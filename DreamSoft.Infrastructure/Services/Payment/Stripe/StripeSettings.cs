using DreamSoft.Application.Common.Interfaces;

namespace DreamSoft.Infrastructure.Services.Payment.Stripe;

/// <summary>
/// Strongly-typed configuration bound from the "Stripe" section in appsettings.json.
/// Injected via IOptions&lt;StripeSettings&gt; into StripePaymentGateway.
/// </summary>
public class StripeSettings : IPaymentSettings
{
    public const string SectionName = "Stripe";

    /// <summary>Stripe secret key (sk_live_... or sk_test_...).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Webhook signing secret from the Stripe dashboard (whsec_...).</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Base URL of the frontend app, no trailing slash. Success/cancel
    /// redirect URLs are composed per-request by the command handlers.
    /// </summary>
    public string FrontendBaseUrl { get; set; } = string.Empty;
}
