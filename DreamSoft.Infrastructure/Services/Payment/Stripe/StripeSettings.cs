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

    /// <summary>Frontend URL Stripe redirects to after a successful payment.</summary>
    public string SuccessUrl { get; set; } = string.Empty;

    /// <summary>Frontend URL Stripe redirects to when the user cancels checkout.</summary>
    public string CancelUrl { get; set; } = string.Empty;
}
