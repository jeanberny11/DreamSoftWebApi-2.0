namespace DreamSoft.Application.Features.Authentication.RegisterTenant;

/// <summary>
/// Response indicating subdomain availability
/// </summary>
public class CheckSubdomainResponse
{
    public bool Available { get; set; }
    public string Subdomain { get; set; } = null!;
    public string? Message { get; set; }
}