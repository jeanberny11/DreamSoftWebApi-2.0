using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Authentication.Requests;
using MediatR;

namespace DreamSoft.Application.Features.Authentication.RegisterTenant;

/// <summary>
/// Handler for checking subdomain availability
/// </summary>
public class CheckSubdomainQueryHandler(IApplicationDbContext context) : IRequestHandler<CheckSubdomainRequest, CheckSubdomainResponse>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<CheckSubdomainResponse> Handle(
        CheckSubdomainRequest request,
        CancellationToken cancellationToken)
    {
        // Check if subdomain exists in database
        var exists = await _context.Tenants
            .AnyAsync(t => t.Subdomain == request.Subdomain.ToLowerInvariant(), cancellationToken);

        if (exists)
        {
            return new CheckSubdomainResponse
            {
                Available = false,
                Subdomain = request.Subdomain,
                Message = $"The subdomain '{request.Subdomain}' is already taken. Please choose another."
            };
        }

        return new CheckSubdomainResponse
        {
            Available = true,
            Subdomain = request.Subdomain,
            Message = $"The subdomain '{request.Subdomain}' is available!"
        };
    }
}