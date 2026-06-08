using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

/// <summary>
/// Base controller for all landing endpoints.
/// All routes are prefixed with /api/v{version}/landing/
/// and require a valid Tenant JWT token.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/landing/[controller]")]
[Authorize(Policy = AuthPolicies.TenantOnly)]
public abstract class LandingControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
