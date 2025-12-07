using DreamSoft.Application.Common.Validators;
using FluentValidation;

namespace DreamSoft.Application.Features.Authentication.VerifyCode;

/// <summary>
/// Validator for VerifyCodeRequest
/// </summary>
public class VerifyCodeValidator : AbstractValidator<VerifyCodeRequest>
{
    public VerifyCodeValidator()
    {
        RuleFor(x => x.Email)
            .BasicEmail(); // ✅ Using custom validator

        RuleFor(x => x.Code)
            .VerificationCode(); // ✅ Using custom validator
    }
}