using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using DreamSoft.Infrastructure.Persistence;
using DreamSoft.Infrastructure.Persistence.Repositories;
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

        // ── Repositories ────────────────────────────────────────────────────

        // Core business repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ITenantRegistrationTokenRepository, TenantRegistrationTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleTemplateRepository, RoleTemplateRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<ISolutionRepository, SolutionRepository>();
        services.AddScoped<ITenantStatusRepository, TenantStatusRepository>();
        services.AddScoped<ISubscriptionStatusRepository, SubscriptionStatusRepository>();

        // Junction entity repositories
        services.AddScoped<IRoleMenuOptionRepository, RoleMenuOptionRepository>();
        services.AddScoped<IRoleMenuOptionTemplateRepository, RoleMenuOptionTemplateRepository>();
        services.AddScoped<IRoleOptionActionRepository, RoleOptionActionRepository>();
        services.AddScoped<IRoleOptionActionTemplateRepository, RoleOptionActionTemplateRepository>();
        services.AddScoped<ISolutionMenuOptionRepository, SolutionMenuOptionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        // Lookup repositories
        services.AddScoped<IBillingCycleRepository, BillingCycleRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IGenderRepository, GenderRepository>();
        services.AddScoped<IIdTypeRepository, IdTypeRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<IMenuGroupRepository, MenuGroupRepository>();
        services.AddScoped<IMenuOptionRepository, MenuOptionRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IMunicipalityRepository, MunicipalityRepository>();
        services.AddScoped<IOptionActionRepository, OptionActionRepository>();
        services.AddScoped<IProvinceRepository, ProvinceRepository>();

        // ── Common Services ──────────────────────────────────────────────────
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
