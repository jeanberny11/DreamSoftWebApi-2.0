using DreamSoft.Application.Features.Apps.LandingApp.Subscription.CancelSubscription;
using DreamSoft.Application.Features.Apps.LandingApp.Subscription.ChangePlan;
using DreamSoft.Application.Features.Apps.LandingApp.Subscription.CreateSubscription;
using DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetChangePlanPreview;
using DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetSubscriptionById;
using DreamSoft.Application.Features.Apps.LandingApp.Subscription.GetSubscriptions;
using DreamSoft.Application.Features.Apps.LandingApp.Subscription.RetryPayment;
using DreamSoft.Application.Features.Apps.LandingApp.Subscription.ResumeSubscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

[Authorize(Policy = AuthPolicies.TenantOnly)]
public class SubscriptionController : LandingControllerBase
{
    // ── GET /api/v1/landing/subscription ─────────────────────────────────────

    /// <summary>
    /// Returns the authenticated tenant's subscriptions across all solutions,
    /// excluding cancelled ones.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TenantSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSubscriptionsQuery(language), cancellationToken);
        return Ok(result);
    }

    // ── GET /api/v1/landing/subscription/{subscriptionId} ────────────────────────

    /// <summary>
    /// Returns a single subscription belonging to the authenticated tenant,
    /// regardless of status (including cancelled). Used by the Manage page.
    /// </summary>
    [HttpGet("{subscriptionId:int}")]
    [ProducesResponseType(typeof(TenantSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int subscriptionId,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetSubscriptionByIdQuery(subscriptionId, language), cancellationToken);
        return Ok(result);
    }

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

    // ── POST /api/v1/subscription/change-plan/preview ────────────────────────

    /// <summary>
    /// Previews the financial impact of a plan change before the tenant
    /// commits — exact same eligibility rules as /change-plan.
    /// </summary>
    [HttpPost("change-plan/preview")]
    [ProducesResponseType(typeof(ChangePlanPreviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PreviewChangePlan(
        [FromBody] GetChangePlanPreviewQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
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

    // ── POST /api/v1/subscription/resume ─────────────────────────────────────

    /// <summary>
    /// Reverts a pending period-end cancellation — the tenant keeps their
    /// subscription and it continues renewing normally.
    /// </summary>
    [HttpPost("resume")]
    [ProducesResponseType(typeof(ResumeSubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Resume(
        [FromBody] ResumeSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
