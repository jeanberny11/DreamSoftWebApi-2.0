using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // TenantEntity fields
        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(c => c.UpdatedBy)
            .HasColumnName("updated_by");

        // Customer-specific fields
        builder.Property(c => c.CustomerTypeId)
            .HasColumnName("customer_type_id")
            .IsRequired();

        builder.Property(c => c.CustomerStatusId)
            .HasColumnName("status")
            .IsRequired();

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

        builder.Property(c => c.TaxId)
            .HasColumnName("tax_id")
            .HasMaxLength(50);

        builder.Property(c => c.IdTypeId)
            .HasColumnName("id_type_id");

        builder.Property(c => c.TaxClassificationId)
            .HasColumnName("tax_classification_id");

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

        builder.Property(c => c.CreditLimit)
            .HasColumnName("credit_limit")
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(c => c.PaymentTerms)
            .HasColumnName("payment_terms")
            .HasMaxLength(50);

        builder.Property(c => c.DiscountPercentage)
            .HasColumnName("discount_percentage")
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0);

        builder.Property(c => c.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("DOP");

        builder.Property(c => c.CustomerCategory)
            .HasColumnName("customer_category")
            .HasMaxLength(100);

        builder.Property(c => c.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        // Audit fields
        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(c => c.TenantId).HasDatabaseName("idx_customers_tenant");
        builder.HasIndex(c => new { c.TenantId, c.CustomerTypeId }).HasDatabaseName("idx_customers_type");
        builder.HasIndex(c => new { c.TenantId, c.CustomerStatusId }).HasDatabaseName("idx_customers_status");
        builder.HasIndex(c => new { c.TenantId, c.TaxId }).HasDatabaseName("idx_customers_tax_id");
        builder.HasIndex(c => c.CountryId).HasDatabaseName("idx_customers_country");
        builder.HasIndex(c => c.ProvinceId).HasDatabaseName("idx_customers_province");
        builder.HasIndex(c => c.MunicipalityId).HasDatabaseName("idx_customers_municipality");
        builder.HasIndex(c => c.TaxClassificationId).HasDatabaseName("idx_customers_tax_classification");

        // Relationships
        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("customers_tenant_id_fkey");

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("customers_created_by_fkey");

        builder.HasOne(c => c.UpdatedByUser)
            .WithMany()
            .HasForeignKey(c => c.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("customers_updated_by_fkey");

        builder.HasOne(c => c.CustomerType)
            .WithMany(ct => ct.Customers)
            .HasForeignKey(c => c.CustomerTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("customers_customer_type_id_fkey");

        builder.HasOne(c => c.Status)
            .WithMany(cs => cs.Customers)
            .HasForeignKey(c => c.CustomerStatusId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("customers_status_fkey");

        builder.HasOne(c => c.IdType)
            .WithMany(it => it.Customers)
            .HasForeignKey(c => c.IdTypeId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("customers_id_type_id_fkey");

        builder.HasOne(c => c.TaxClassification)
            .WithMany(tc => tc.Customers)
            .HasForeignKey(c => c.TaxClassificationId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("customers_tax_classification_id_fkey");

        builder.HasOne(c => c.Country)
            .WithMany()
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("customers_country_id_fkey");

        builder.HasOne(c => c.Province)
            .WithMany()
            .HasForeignKey(c => c.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("customers_province_id_fkey");

        builder.HasOne(c => c.Municipality)
            .WithMany()
            .HasForeignKey(c => c.MunicipalityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("customers_municipality_id_fkey");
    }
}
