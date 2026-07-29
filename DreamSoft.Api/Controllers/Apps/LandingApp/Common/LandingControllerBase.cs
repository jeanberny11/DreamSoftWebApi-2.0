using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

/// <summary>
/// Base controller for all landing endpoints.
/// All routes are prefixed with /api/v{version}/landing/
/// Authorization is applied per-controller or per-action:
///   - Public endpoints use [AllowAnonymous]
///   - Protected endpoints use [Authorize(Policy = AuthPolicies.TenantOnly)]
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/landing/[controller]")]
public abstract class LandingControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
