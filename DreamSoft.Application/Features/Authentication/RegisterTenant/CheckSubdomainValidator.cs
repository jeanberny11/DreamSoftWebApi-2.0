using DreamSoft.Application.Common.Validators;
using DreamSoft.Application.Features.Authentication.Requests;
using FluentValidation;

namespace DreamSoft.Application.Features.Authentication.Validators;

/// <summary>
/// Validator for CheckSubdomainRequest
/// </summary>
public class CheckSubdomainValidator : AbstractValidator<CheckSubdomainRequest>
{
    public CheckSubdomainValidator()
    {
        RuleFor(x => x.Subdomain)
            .SubdomainFormat(); // ✅ Using custom validator (includes reserved words check)
    }
}