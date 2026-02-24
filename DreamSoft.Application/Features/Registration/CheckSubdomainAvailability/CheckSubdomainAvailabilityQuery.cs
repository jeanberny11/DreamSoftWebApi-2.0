using MediatR;

namespace DreamSoft.Application.Features.Registration.CheckSubdomainAvailability;

/// <summary>
/// Returns whether a subdomain is available for registration.
/// </summary>
public record CheckSubdomainAvailabilityQuery(string Subdomain)
    : IRequest<SubdomainAvailabilityResponse>;

/// <summary>
/// Result DTO — tells the client whether the subdomain is available
/// and provides a normalised version of the subdomain.
/// </summary>
public record SubdomainAvailabilityResponse(
    string Subdomain,
    bool IsAvailable,
    string? Reason = null);
