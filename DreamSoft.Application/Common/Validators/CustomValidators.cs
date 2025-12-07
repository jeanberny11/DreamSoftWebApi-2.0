using FluentValidation;

namespace DreamSoft.Application.Common.Validators;

/// <summary>
/// Custom reusable validation rules for the entire application
/// </summary>
public static class CustomValidators
{
    #region Email Validation

    /// <summary>
    /// Validates email format with disposable domain check
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters")
            .Must(NotBeDisposableEmail)
            .WithMessage("Disposable email addresses are not allowed");
    }

    /// <summary>
    /// Validates basic email format without disposable domain check
    /// </summary>
    public static IRuleBuilderOptions<T, string> BasicEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters");
    }

    private static bool NotBeDisposableEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Block temporary/disposable email domains
        var disposableDomains = new[]
        {
            "tempmail.com",
            "throwaway.email",
            "guerrillamail.com",
            "10minutemail.com",
            "mailinator.com",
            "maildrop.cc",
            "yopmail.com",
            "temp-mail.org",
            "fakeinbox.com",
            "trashmail.com",
            "getnada.com",
            "dispostable.com"
        };

        var domain = email.Split('@').LastOrDefault()?.ToLower();

        if (string.IsNullOrWhiteSpace(domain))
            return false;

        return !disposableDomains.Contains(domain);
    }

    #endregion

    #region Password Validation

    /// <summary>
    /// Validates strong password requirements (for registration)
    /// </summary>
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .MaximumLength(100)
            .WithMessage("Password must not exceed 100 characters")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one number")
            .Matches(@"[@$!%*?&#]")
            .WithMessage("Password must contain at least one special character (@$!%*?&#)");
    }

    /// <summary>
    /// Validates basic password (for login - just required and length)
    /// </summary>
    public static IRuleBuilderOptions<T, string> BasicPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .MaximumLength(100)
            .WithMessage("Password must not exceed 100 characters");
    }

    /// <summary>
    /// Validates password confirmation matches password
    /// </summary>
    public static IRuleBuilderOptions<T, string> PasswordConfirmation<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Func<T, string> passwordSelector)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Password confirmation is required")
            .Must((model, confirmPassword) => confirmPassword == passwordSelector(model))
            .WithMessage("Passwords do not match");
    }

    #endregion

    #region Subdomain Validation

    /// <summary>
    /// Validates subdomain format
    /// </summary>
    public static IRuleBuilderOptions<T, string> SubdomainFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Subdomain is required")
            .Length(3, 50)
            .WithMessage("Subdomain must be between 3 and 50 characters")
            .Matches(@"^[a-z][a-z0-9-]*[a-z0-9]$")
            .WithMessage("Subdomain must start with a letter, contain only lowercase letters, numbers, and hyphens, and cannot end with a hyphen")
            .NotReservedSubdomain();
    }

    /// <summary>
    /// Checks if subdomain is not a reserved word
    /// </summary>
    public static IRuleBuilderOptions<T, string> NotReservedSubdomain<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(NotBeReservedSubdomain)
            .WithMessage("This subdomain is reserved and cannot be used");
    }

    private static bool NotBeReservedSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return false;

        var reservedWords = new[]
        {
            "admin",
            "api",
            "www",
            "app",
            "mail",
            "ftp",
            "localhost",
            "test",
            "demo",
            "staging",
            "dev",
            "development",
            "production",
            "prod",
            "beta",
            "alpha",
            "support",
            "help",
            "docs",
            "blog",
            "cdn",
            "static",
            "assets",
            "public",
            "private",
            "secure",
            "ssl",
            "vpn",
            "root",
            "system",
            "internal",
            "backup",
            "config",
            "database",
            "db",
            "file",
            "files",
            "image",
            "images",
            "img",
            "media",
            "portal",
            "server",
            "service",
            "services",
            "web",
            "webmail"
        };

        return !reservedWords.Contains(subdomain.ToLower());
    }

    #endregion

    #region Name Validation

    /// <summary>
    /// Validates person name (allows international characters)
    /// </summary>
    public static IRuleBuilderOptions<T, string> PersonName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Name is required")
            .Length(2, 100)
            .WithMessage("Name must be between 2 and 100 characters")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\-\']+$")
            .WithMessage("Name contains invalid characters");
    }

    /// <summary>
    /// Validates company name
    /// </summary>
    public static IRuleBuilderOptions<T, string> CompanyName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Company name is required")
            .Length(2, 255)
            .WithMessage("Company name must be between 2 and 255 characters")
            .Matches(@"^[a-zA-Z0-9\s\-\.\,&]+$")
            .WithMessage("Company name contains invalid characters");
    }

    #endregion

    #region Phone Validation

    /// <summary>
    /// Validates international phone format (E.164)
    /// </summary>
    public static IRuleBuilderOptions<T, string> InternationalPhone<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in valid international format (E.164)");
    }

    #endregion

    #region Verification Code Validation

    /// <summary>
    /// Validates 6-digit verification code
    /// </summary>
    public static IRuleBuilderOptions<T, string> VerificationCode<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Verification code is required")
            .Length(6)
            .WithMessage("Verification code must be exactly 6 digits")
            .Matches(@"^\d{6}$")
            .WithMessage("Verification code must contain only numbers");
    }

    #endregion

    #region Other Common Validations

    /// <summary>
    /// Validates tax ID/RNC
    /// </summary>
    public static IRuleBuilderOptions<T, string> TaxId<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(50)
            .WithMessage("Tax ID must not exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9-]+$")
            .WithMessage("Tax ID contains invalid characters");
    }

    /// <summary>
    /// Validates positive integer ID
    /// </summary>
    public static IRuleBuilderOptions<T, int> PositiveId<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0");
    }

    /// <summary>
    /// Validates terms acceptance
    /// </summary>
    public static IRuleBuilderOptions<T, bool> MustAcceptTerms<T>(this IRuleBuilder<T, bool> ruleBuilder)
    {
        return ruleBuilder
            .Equal(true)
            .WithMessage("You must accept the terms and conditions");
    }

    #endregion
}