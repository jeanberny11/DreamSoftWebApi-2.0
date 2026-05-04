namespace DreamSoft.Application.Features.Admin.Auth;

public record AdminLoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int AdminUserId,
    string Email,
    string FullName,
    string RoleCode);
