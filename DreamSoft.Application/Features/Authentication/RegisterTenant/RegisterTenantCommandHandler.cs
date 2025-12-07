using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Authentication.RegisterTenant;
using DreamSoft.Application.Features.Authentication.Requests;
using DreamSoft.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Application.Features.Authentication.Commands.RegisterTenant;

/// <summary>
/// Handler for tenant and user registration
/// </summary>
public class RegisterTenantCommandHandler
    : IRequestHandler<RegisterTenantRequest, RegisterTenantResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterTenantCommandHandler> _logger;

    public RegisterTenantCommandHandler(
        IApplicationDbContext context,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        ILogger<RegisterTenantCommandHandler> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<RegisterTenantResponse> Handle(
        RegisterTenantRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Tenant registration started for subdomain: {Subdomain}",
            request.Subdomain);

        // 1. Validate session token and extract email
        var sessionData = _jwtService.ValidateSessionToken(request.SessionToken);
        if (sessionData == null)
        {
            _logger.LogWarning("Invalid or expired session token provided during registration");
            throw new UnauthorizedException("Session has expired. Please verify your email again.");
        }

        var email = sessionData.Email.ToLowerInvariant();
        var subdomain = request.Subdomain.ToLowerInvariant();

        // 2. Check if email already exists (defensive check)
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailExists)
        {
            _logger.LogWarning(
                "Attempted registration with existing email: {Email}",
                email);

            throw new ConflictException($"An account with email '{email}' already exists.");
        }

        // 3. Check if subdomain is available
        var subdomainExists = await _context.Tenants
            .AnyAsync(t => t.Subdomain == subdomain, cancellationToken);

        if (subdomainExists)
        {
            _logger.LogWarning(
                "Attempted registration with existing subdomain: {Subdomain}",
                subdomain);

            throw new ConflictException($"The subdomain '{subdomain}' is already taken.");
        }

        // 4. Verify language exists
        var languageExists = await _context.Languages
            .AnyAsync(l => l.LanguageId == request.LanguageId && l.Active, cancellationToken);

        if (!languageExists)
        {
            throw new ValidationException("Invalid language selection.");
        }

        // 5. Start database transaction
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 6. Create Tenant
            var tenant = Tenant.Create(
                companyName: request.CompanyName,
                subdomain: subdomain,
                phone: request.Phone,
                taxId: request.TaxId,
                languageId: request.LanguageId);

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Tenant created successfully. TenantId: {TenantId}, TenantNumber: {TenantNumber}",
                tenant.TenantId, tenant.TenantNumber);

            // 7. Hash password
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // 8. Create User (admin user for the tenant)
            var user = User.Create(
                tenantId: tenant.TenantId,
                email: email,
                username: email, // Use email as username initially
                passwordHash: passwordHash,
                firstName: request.FirstName,
                lastName: request.LastName,
                languageId: request.LanguageId,
                isAdmin: true); // First user is always admin

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User created successfully. UserId: {UserId}, Email: {Email}",
                user.UserId, user.Email);

            // 9. Commit transaction
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Registration completed successfully for tenant: {TenantNumber}, user: {Email}",
                tenant.TenantNumber, email);

            // 10. Generate access token
            var accessToken = _jwtService.GenerateAccessToken(
                userId: user.UserId,
                tenantId: tenant.TenantId,
                email: user.Email,
                username: user.Username,
                isAdmin: user.IsAdmin);

            // 11. Send welcome email (fire and forget - don't block response)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendWelcomeEmailAsync(
                        email,
                        request.FirstName,
                        request.CompanyName,
                        subdomain);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email to: {Email}", email);
                }
            }, cancellationToken);

            // 12. Return success response
            return new RegisterTenantResponse
            {
                Success = true,
                Message = "Registration completed successfully! Welcome to DreamSoft.",
                TenantId = tenant.TenantId,
                TenantNumber = tenant.TenantNumber,
                CompanyName = tenant.CompanyName,
                Subdomain = tenant.Subdomain,
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AccessToken = accessToken,
                AccessTokenExpiresInSeconds = 3600 // 1 hour
            };
        }
        catch (Exception ex)
        {
            // Rollback transaction on any error
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex,
                "Registration failed for subdomain: {Subdomain}, email: {Email}",
                subdomain, email);

            throw;
        }
    }
}