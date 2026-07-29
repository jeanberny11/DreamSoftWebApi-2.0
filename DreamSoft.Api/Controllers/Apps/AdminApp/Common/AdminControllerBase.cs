using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

/// <summary>
/// Base controller for all admin endpoints.
/// All routes are prefixed with /api/v{version}/admin/
/// and require a valid SuperAdmin JWT token.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/[controller]")]
[Authorize(Policy = AuthPolicies.SuperAdminOnly)]
public abstract class AdminControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
