using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("tenants");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(t => t.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false);

        builder.Property(t => t.EmailVerifiedAt)
            .HasColumnName("email_verified_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.Website)
            .HasColumnName("website")
            .HasMaxLength(255)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.AddressLine1)
            .HasColumnName("address_line1")
            .HasMaxLength(255)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.AddressLine2)
            .HasColumnName("address_line2")
            .HasMaxLength(255)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.CountryId)
            .HasColumnName("country_id");

        builder.Property(t => t.ProvinceId)
            .HasColumnName("province_id");

        builder.Property(t => t.MunicipalityId)
            .HasColumnName("municipality_id");

        builder.Property(t => t.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.LanguageId)
            .HasColumnName("language_id")
            .IsRequired();

        builder.Property(t => t.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.StatusId)
            .HasColumnName("status_id")
            .IsRequired();

        builder.Property(t => t.StripeCustomerId)
            .HasColumnName("stripe_customer_id")
            .HasMaxLength(100);

        builder.Property(t => t.TermsVersion)
            .HasColumnName("terms_version")
            .HasMaxLength(50);

        builder.Property(t => t.TermsAcceptedAt)
            .HasColumnName("terms_accepted_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.TermsAcceptedIp)
            .HasColumnName("terms_accepted_ip")
            .HasMaxLength(50);

        builder.Property(t => t.OnboardingCompleted)
            .HasColumnName("onboarding_completed")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.LockoutUntil)
            .HasColumnName("lockout_until")
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.LastLoginAt)
            .HasColumnName("last_login_at")
            .HasColumnType("timestamp with time zone");

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(t => t.Email)
            .IsUnique()
            .HasDatabaseName("tenants_email_key");

        builder.HasIndex(t => t.StripeCustomerId)
            .HasDatabaseName("idx_tenants_stripe_customer_id");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(t => t.Country)
            .WithMany(c => c.Tenants)
            .HasForeignKey(t => t.CountryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.Province)
            .WithMany(p => p.Tenants)
            .HasForeignKey(t => t.ProvinceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.Municipality)
            .WithMany(m => m.Tenants)
            .HasForeignKey(t => t.MunicipalityId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.Language)
            .WithMany(l => l.Tenants)
            .HasForeignKey(t => t.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Status)
            .WithMany(s => s.Tenants)
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.TenantSubdomains)
            .WithOne(td => td.Tenant)
            .HasForeignKey(td => td.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.TenantSubscriptions)
            .WithOne(ts => ts.Tenant)
            .HasForeignKey(ts => ts.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.RefreshTokens)
            .WithOne(rt => rt.Tenant)
            .HasForeignKey(rt => rt.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.RegistrationTokens)
            .WithOne(rt => rt.Tenant)
            .HasForeignKey(rt => rt.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
