using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Tenant entity
/// Maps to the 'tenants' table in PostgreSQL
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Table mapping
        builder.ToTable("tenants");

        // Primary key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
        builder.Property(t => t.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(255)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.Subdomain)
            .HasColumnName("subdomain")
            .HasMaxLength(255)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.TaxId)
            .HasColumnName("tax_id")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.TaxIdVerified)
            .HasColumnName("tax_id_verified")
            .HasDefaultValue(false);

        builder.Property(t => t.TaxIdVerifiedAt)
            .HasColumnName("tax_id_verified_at");

        builder.Property(t => t.TaxIdVerifiedBy)
            .HasColumnName("tax_id_verified_by");

        builder.Property(t => t.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false);

        builder.Property(t => t.EmailVerifiedAt)
            .HasColumnName("email_verified_at");

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

        builder.Property(t => t.CurrencyId)
            .HasColumnName("currency_id")
            .IsRequired();

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

        // Audit fields
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

        // Relationships
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

        builder.HasOne(t => t.Currency)
            .WithMany(c => c.Tenants)
            .HasForeignKey(t => t.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Language)
            .WithMany(l => l.Tenants)
            .HasForeignKey(t => t.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Status)
            .WithMany(s => s.Tenants)
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Roles)
            .WithOne(r => r.Tenant)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
