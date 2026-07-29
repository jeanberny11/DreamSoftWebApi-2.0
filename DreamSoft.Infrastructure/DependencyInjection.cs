using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using DreamSoft.Infrastructure.Persistence;
using DreamSoft.Infrastructure.Persistence.Repositories;
using DreamSoft.Infrastructure.Services.Common;
using DreamSoft.Infrastructure.Services.Features.Email;
using DreamSoft.Infrastructure.Services.Payment;
using DreamSoft.Infrastructure.Services.Payment.Stripe;
using DreamSoft.Infrastructure.Services.RateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Stripe;
using TokenService = DreamSoft.Infrastructure.Services.Common.TokenService;

namespace DreamSoft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ──────────────────────────────────────────────────────────
        // Prefer DATABASE_URL (injected by Railway) over the legacy key-value
        // connection string. Falls back to ConnectionStrings:DefaultConnection
        // for local development.
        var databaseUrl =
            configuration["DATABASE_URL"].NullIfEmpty() ??
            configuration.GetConnectionString("DefaultConnection").NullIfEmpty() ??
            throw new InvalidOperationException(
                "No database connection configured. Set DATABASE_URL or ConnectionStrings:DefaultConnection.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                databaseUrl,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // ── Unit of Work ──────────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Tenant Account Repositories ───────────────────────────────────────
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantRefreshTokenRepository, TenantRefreshTokenRepository>();
        services.AddScoped<ITenantRegistrationTokenRepository, TenantRegistrationTokenRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<ITenantSubdomainRepository, TenantSubdomainRepository>();
        services.AddScoped<ISubscriptionInvoiceRepository, SubscriptionInvoiceRepository>();
        services.AddScoped<ISubscriptionPaymentRepository, SubscriptionPaymentRepository>();
        services.AddScoped<ISubscriptionCancellationLogRepository, SubscriptionCancellationLogRepository>();

        // ── Tenant + Solution Scoped Repositories ─────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleMenuOptionRepository, RoleMenuOptionRepository>();
        services.AddScoped<IRoleOptionActionRepository, RoleOptionActionRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // ── Admin Repositories ────────────────────────────────────────────────
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IAdminRefreshTokenRepository, AdminRefreshTokenRepository>();

        // ── Global Business Repositories ──────────────────────────────────────
        services.AddScoped<ISolutionRepository, SolutionRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IPlanPriceRepository, PlanPriceRepository>();
        services.AddScoped<IPlanLimitRepository, PlanLimitRepository>();
        services.AddScoped<IPlanMenuOptionRepository, PlanMenuOptionRepository>();
        services.AddScoped<IRoleTemplateRepository, RoleTemplateRepository>();
        services.AddScoped<IRoleMenuOptionTemplateRepository, RoleMenuOptionTemplateRepository>();
        services.AddScoped<IRoleOptionActionTemplateRepository, RoleOptionActionTemplateRepository>();

        // ── Lookup Repositories ───────────────────────────────────────────────
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
        services.AddScoped<ITenantStatusRepository, TenantStatusRepository>();
        services.AddScoped<ISubscriptionStatusRepository, SubscriptionStatusRepository>();
        services.AddScoped<ICustomerTypeRepository, CustomerTypeRepository>();
        services.AddScoped<ICustomerStatusRepository, CustomerStatusRepository>();
        services.AddScoped<ITaxClassificationRepository, TaxClassificationRepository>();

        // ── Common Services ───────────────────────────────────────────────────
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();
        services.AddScoped<ICurrentAdminService, CurrentAdminService>();
        services.AddScoped<IRequestLanguageService, RequestLanguageService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();

        // ── Email Service ─────────────────────────────────────────────────────
        services.AddHttpClient("Resend");
        services.AddScoped<IEmailService, EmailService>();

        // ── Payment Gateway ───────────────────────────────────────────────────
        services.Configure<StripeSettings>(
            configuration.GetSection(StripeSettings.SectionName));
        StripeConfiguration.ApiKey =
            configuration[$"{StripeSettings.SectionName}:SecretKey"];
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddSingleton<IPaymentSettings>(sp =>
            sp.GetRequiredService<IOptions<StripeSettings>>().Value);

        // ── Redis + Webhook Event Store ───────────────────────────────────────
        // Singleton: StackExchange.Redis ConnectionMultiplexer is thread-safe
        // and designed to be shared across the application lifetime.
        // Prefer REDIS_URL (injected by Railway) over ConnectionStrings:Redis.
        var redisConnection =
            configuration["REDIS_URL"].NullIfEmpty() ??
            configuration.GetConnectionString("Redis").NullIfEmpty() ??
            "localhost:6379";
        var redisOptions = ConfigurationOptions.Parse(redisConnection);
        redisOptions.AbortOnConnectFail = false;
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisOptions));
        services.AddScoped<IWebhookEventStore, RedisWebhookEventStore>();

        // ── Rate Limiting ─────────────────────────────────────────────────────
        // Singleton so the in-memory window state persists across requests
        services.AddSingleton<IRateLimitService, InMemoryRateLimitService>();

        // ── HTTP Context ──────────────────────────────────────────────────────
        services.AddHttpContextAccessor();

        return services;
    }
}
