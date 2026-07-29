namespace DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.Dtos;
public record TenantAuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int TenantId,
    string Email,
    string FirstName,
    string LastName,
    string LogoUrl,
    bool OnboardingCompleted,
    bool EmailVerified,
    string TenantStatusCode);