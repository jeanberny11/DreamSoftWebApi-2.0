using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("customers", t =>
        {
            // Mirrors DB check constraint: at least one contact field must be set
            t.HasCheckConstraint(
                "check_customer_contact_info",
                "email IS NOT NULL OR phone IS NOT NULL OR mobile IS NOT NULL");
        });

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Tenant + Solution ─────────────────────────────────────────────
        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(c => c.SolutionId)
            .HasColumnName("solution_id")
            .IsRequired();

        // ── Identity ──────────────────────────────────────────────────────
        builder.Property(c => c.CustomerTypeId)
            .HasColumnName("customer_type_id")
            .IsRequired();

        builder.Property(c => c.CustomerStatusId)
            .HasColumnName("customer_status_id")
            .IsRequired();

        // ── Personal / Company Info ───────────────────────────────────────
        builder.Property(c => c.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100);

        builder.Property(c => c.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(255);

        builder.Property(c => c.CommercialName)
            .HasColumnName("commercial_name")
            .HasMaxLength(255);

        builder.Property(c => c.ContactPerson)
            .HasColumnName("contact_person")
            .HasMaxLength(255);

        // ── Tax Info ──────────────────────────────────────────────────────
        builder.Property(c => c.TaxId)
            .HasColumnName("tax_id")
            .HasMaxLength(50);

        builder.Property(c => c.IdTypeId)
            .HasColumnName("id_type_id");

        builder.Property(c => c.TaxClassificationId)
            .HasColumnName("tax_classification_id");

        // ── Contact Info ──────────────────────────────────────────────────
        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.Property(c => c.Mobile)
            .HasColumnName("mobile")
            .HasMaxLength(20);

        builder.Property(c => c.Website)
            .HasColumnName("website")
            .HasMaxLength(255);

        // ── Address ───────────────────────────────────────────────────────
        builder.Property(c => c.AddressLine1)
            .HasColumnName("address_line1")
            .HasMaxLength(255);

        builder.Property(c => c.AddressLine2)
            .HasColumnName("address_line2")
            .HasMaxLength(255);

        builder.Property(c => c.CountryId)
            .HasColumnName("country_id")
            .IsRequired();

        builder.Property(c => c.ProvinceId)
            .HasColumnName("province_id")
            .IsRequired();

        builder.Property(c => c.MunicipalityId)
            .HasColumnName("municipality_id")
            .IsRequired();

        builder.Property(c => c.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(20);

        // ── Commercial ────────────────────────────────────────────────────
        builder.Property(c => c.CreditLimit)
            .HasColumnName("credit_limit")
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(c => c.PaymentTerms)
            .HasColumnName("payment_terms")
            .HasMaxLength(50);

        builder.Property(c => c.DiscountPercentage)
            .HasColumnName("discount_percentage")
            .HasPrecision(5, 2)
            .HasDefaultValue(0m);

        builder.Property(c => c.CurrencyId)
            .HasColumnName("currency_id")
            .IsRequired();

        builder.Property(c => c.CustomerCategory)
            .HasColumnName("customer_category")
            .HasMaxLength(50);

        // ── Notes ─────────────────────────────────────────────────────────
        builder.Property(c => c.Notes)
            .HasColumnName("notes");

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(c => c.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(c => c.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(c => new { c.TenantId, c.SolutionId })
            .HasDatabaseName("idx_customers_tenant_solution");

        builder.HasIndex(c => new { c.TenantId, c.SolutionId, c.IsActive })
            .HasDatabaseName("idx_customers_tenant_solution_is_active");

        builder.HasIndex(c => new { c.TenantId, c.SolutionId, c.TaxId })
            .HasDatabaseName("idx_customers_tax_id");

        builder.HasIndex(c => c.CountryId)
            .HasDatabaseName("idx_customers_country");

        builder.HasIndex(c => c.ProvinceId)
            .HasDatabaseName("idx_customers_province");

        builder.HasIndex(c => c.MunicipalityId)
            .HasDatabaseName("idx_customers_municipality");

        builder.HasIndex(c => c.TaxClassificationId)
            .HasDatabaseName("idx_customers_tax_classification");

        builder.HasIndex(c => c.CreatedBy)
            .HasDatabaseName("idx_customers_created_by");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("idx_customers_is_active");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Solution)
            .WithMany()
            .HasForeignKey(c => c.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CustomerType)
            .WithMany(ct => ct.Customers)
            .HasForeignKey(c => c.CustomerTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CustomerStatus)
            .WithMany(cs => cs.Customers)
            .HasForeignKey(c => c.CustomerStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.IdType)
            .WithMany()
            .HasForeignKey(c => c.IdTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.TaxClassification)
            .WithMany(tc => tc.Customers)
            .HasForeignKey(c => c.TaxClassificationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Country)
            .WithMany(co => co.Customers)
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Province)
            .WithMany(p => p.Customers)
            .HasForeignKey(c => c.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Municipality)
            .WithMany()
            .HasForeignKey(c => c.MunicipalityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Currency)
            .WithMany()
            .HasForeignKey(c => c.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.UpdatedByUser)
            .WithMany()
            .HasForeignKey(c => c.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
