using DreamSoft.Application.Features.Subscription.CancelSubscription;
using DreamSoft.Application.Features.Subscription.ChangePlan;
using DreamSoft.Application.Features.Subscription.CreateSubscription;
using DreamSoft.Application.Features.Subscription.RetryPayment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

[Authorize(Policy = AuthPolicies.TenantOnly)]
public class SubscriptionController : ApiControllerBase
{
    // ── POST /api/v1/subscription/create ─────────────────────────────────────

    /// <summary>
    /// Creates a subscription for a tenant, provisioning the subdomain,
    /// admin role with full permissions, and the admin user account.
    /// Returns a CheckoutUrl — the frontend must redirect the user to Stripe.
    /// The subscription is activated asynchronously via the Stripe webhook.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateSubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // ── POST /api/v1/subscription/retry-payment ───────────────────────────────

    /// <summary>
    /// Creates a new Stripe checkout session for an existing subscription that
    /// is in PROCESSING_PAYMENT or PAYMENT_FAILED status.
    /// Returns a CheckoutUrl — the frontend must redirect the user to this URL.
    /// </summary>
    [HttpPost("retry-payment")]
    [ProducesResponseType(typeof(RetryPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RetryPayment(
        [FromBody] RetryPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // ── POST /api/v1/subscription/change-plan ────────────────────────────────

    /// <summary>
    /// Upgrades or downgrades the tenant's current active subscription to a new
    /// plan or billing cycle.
    /// </summary>
    [HttpPost("change-plan")]
    [ProducesResponseType(typeof(ChangePlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangePlan(
        [FromBody] ChangePlanCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // ── POST /api/v1/subscription/cancel ─────────────────────────────────────

    /// <summary>
    /// Cancels the tenant's active subscription for the specified solution.
    /// </summary>
    [HttpPost("cancel")]
    [ProducesResponseType(typeof(CancelSubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        [FromBody] CancelSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
