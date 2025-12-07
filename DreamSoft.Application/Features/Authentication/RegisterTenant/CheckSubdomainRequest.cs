using DreamSoft.Application.Features.Authentication.RegisterTenant;
using MediatR;

namespace DreamSoft.Application.Features.Authentication.Requests;

/// <summary>
/// Request to check subdomain availability
/// </summary>
public class CheckSubdomainRequest : IRequest<CheckSubdomainResponse>
{
    public string Subdomain { get; set; } = null!;
}