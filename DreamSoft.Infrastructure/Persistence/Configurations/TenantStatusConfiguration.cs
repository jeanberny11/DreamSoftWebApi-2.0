using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class TenantStatusConfiguration : IEntityTypeConfiguration<TenantStatus>
{
    public void Configure(EntityTypeBuilder<TenantStatus> builder)
    {
        builder.ToTable("tenant_statuses");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        // Configure TranslatedString as JSONB
        builder.OwnsOne(t => t.Name, name =>
        {
            name.ToJson("translations"); // Maps to JSONB column

            name.Property(ts => ts.Spanish)
                .HasJsonPropertyName("es")
                .IsRequired();

            name.Property(ts => ts.English)
                .HasJsonPropertyName("en");
        });

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(t => t.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(t => t.Code)
            .IsUnique()
            .HasDatabaseName("idx_tenant_statuses_code");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("idx_tenant_statuses_is_active");

    }
}