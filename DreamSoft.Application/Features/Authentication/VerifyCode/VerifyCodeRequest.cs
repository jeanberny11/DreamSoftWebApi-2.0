using MediatR;

namespace DreamSoft.Application.Features.Authentication.VerifyCode;

/// <summary>
/// Request to verify email code
/// </summary>
public class VerifyCodeRequest:IRequest<VerifyCodeResponse>
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}