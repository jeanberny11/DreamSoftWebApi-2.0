using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Business Identity
        builder.Property(t => t.TenantNumber)
            .HasColumnName("tenant_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Subdomain)
            .HasColumnName("subdomain")
            .HasMaxLength(100)
            .IsRequired();

        // Tax/Legal Information
        builder.Property(t => t.TaxId)
            .HasColumnName("tax_id")
            .HasMaxLength(50);

        builder.Property(t => t.TaxIdVerified)
            .HasColumnName("tax_id_verified")
            .HasDefaultValue(false);

        builder.Property(t => t.TaxIdVerifiedAt)
            .HasColumnName("tax_id_verified_at");

        builder.Property(t => t.TaxIdVerifiedBy)
            .HasColumnName("tax_id_verified_by");

        // Contact Information
        builder.Property(t => t.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false);

        builder.Property(t => t.EmailVerifiedAt)
            .HasColumnName("email_verified_at");

        builder.Property(t => t.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.Property(t => t.Website)
            .HasColumnName("website")
            .HasMaxLength(255);

        // Address
        builder.Property(t => t.AddressLine1)
            .HasColumnName("address_line1")
            .HasMaxLength(255);

        builder.Property(t => t.AddressLine2)
            .HasColumnName("address_line2")
            .HasMaxLength(255);

        builder.Property(t => t.CountryId)
            .HasColumnName("country_id");

        builder.Property(t => t.ProvinceId)
            .HasColumnName("province_id");

        builder.Property(t => t.MunicipalityId)
            .HasColumnName("municipality_id");

        builder.Property(t => t.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(20);

        // Settings
        builder.Property(t => t.IndustryType)
            .HasColumnName("industry_type")
            .HasMaxLength(100);

        builder.Property(t => t.Timezone)
            .HasColumnName("timezone")
            .HasMaxLength(50)
            .HasDefaultValue("America/Santo_Domingo");

        builder.Property(t => t.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("DOP");

        builder.Property(t => t.DefaultLanguage)
            .HasColumnName("default_language")
            .HasMaxLength(10)
            .HasDefaultValue("es");

        // Branding
        builder.Property(t => t.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);

        // Status
        builder.Property(t => t.StatusId)
            .HasColumnName("status_id")
            .IsRequired();

        // Audit Fields (from BaseEntity)
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(t => t.Status)
            .WithMany(s => s.Tenants)
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.TenantNumber)
            .IsUnique()
            .HasDatabaseName("idx_tenants_tenant_number");

        builder.HasIndex(t => t.Subdomain)
            .IsUnique()
            .HasDatabaseName("idx_tenants_subdomain");

        builder.HasIndex(t => t.TaxId)
            .IsUnique()
            .HasDatabaseName("idx_tenants_tax_id")
            .HasFilter("tax_id IS NOT NULL");

        builder.HasIndex(t => t.Email)
            .HasDatabaseName("idx_tenants_email");

        builder.HasIndex(t => t.StatusId)
            .HasDatabaseName("idx_tenants_status_id");

        builder.HasIndex(t => t.CountryId)
            .HasDatabaseName("idx_tenants_country");

        builder.HasIndex(t => t.ProvinceId)
            .HasDatabaseName("idx_tenants_province");
    }
}