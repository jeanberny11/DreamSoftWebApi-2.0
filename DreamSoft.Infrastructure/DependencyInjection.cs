using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using DreamSoft.Infrastructure.Persistence;
using DreamSoft.Infrastructure.Services.Common;
using DreamSoft.Infrastructure.Services.Features.Authentication;
using DreamSoft.Infrastructure.Services.Features.Caching;
using DreamSoft.Infrastructure.Services.Features.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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

        // Feature Services - Authentication
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        // Feature Services - Caching (Redis)
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(
                configuration.GetConnectionString("Redis") ?? "localhost:6379"));
        services.AddScoped<IRedisService, RedisService>();

        // Feature Services - Email (Resend API)
        services.AddHttpClient("Resend");
        services.AddScoped<IEmailService, EmailService>();

        // Required for CurrentUserService to access HTTP context
        services.AddHttpContextAccessor();

        return services;
    }
}