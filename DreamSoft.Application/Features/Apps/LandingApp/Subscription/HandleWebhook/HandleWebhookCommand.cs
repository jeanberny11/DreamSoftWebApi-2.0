using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.HandleWebhook;

/// <summary>
/// Processes an incoming payment gateway webhook.
/// Payload and Signature come directly from the raw HTTP request —
/// the controller must NOT deserialize the body before passing it here.
/// Always returns Unit — the controller replies with 200 OK to the gateway.
/// </summary>
public record HandleWebhookCommand(
    string Payload,
    string Signature
) : IRequest<Unit>;
