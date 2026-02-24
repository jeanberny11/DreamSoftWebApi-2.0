using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using DreamSoft.Infrastructure.Persistence;
using DreamSoft.Infrastructure.Services.Common;
using DreamSoft.Infrastructure.Services.Features.Email;
using DreamSoft.Infrastructure.Services.RateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DreamSoft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext with PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Register UnitOfWork for complex transactions
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Common Services
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();

        // Email Service (Resend API)
        services.AddHttpClient("Resend");
        services.AddScoped<IEmailService, EmailService>();

        // Rate limiting — singleton so the in-memory window state persists across requests
        services.AddSingleton<IRateLimitService, InMemoryRateLimitService>();

        // Required for CurrentUserService to access HTTP context
        services.AddHttpContextAccessor();

        return services;
    }
}