namespace DreamSoft.Application.Features.Authentication.RegisterTenant;

/// <summary>
/// Response after successful tenant registration
/// </summary>
public class RegisterTenantResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;

    // Tenant information
    public int TenantId { get; set; }
    public string TenantNumber { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string Subdomain { get; set; } = null!;

    // User information
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    // Authentication tokens
    public string AccessToken { get; set; } = null!;
    public int AccessTokenExpiresInSeconds { get; set; } // 3600 = 1 hour

    // Refresh token will be in httpOnly cookie (not in response body)
}