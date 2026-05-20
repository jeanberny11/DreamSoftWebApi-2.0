namespace DreamSoft.Application.Features.Apps.AdminApp.Auth;

public record AdminLoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int AdminUserId,
    string Email,
    string FullName,
    string RoleCode);
