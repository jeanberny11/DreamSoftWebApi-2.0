using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for TenantStatus entity
/// Maps to the 'tenant_statuses' table in PostgreSQL
/// </summary>
public class TenantStatusConfiguration : IEntityTypeConfiguration<TenantStatus>
{
    public void Configure(EntityTypeBuilder<TenantStatus> builder)
    {
        // Table mapping
        builder.ToTable("tenant_statuses");

        // Primary key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
        builder.Property(t => t.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        // JSONB Translation Configuration (Name + Description)
        builder.OwnsOne(t => t.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(tr => tr.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(s => s.Name)
                    .HasJsonPropertyName("name")
                    .IsRequired();
                spanish.Property(s => s.Descripcion)
                    .HasJsonPropertyName("description");
            });

            translations.OwnsOne(tr => tr.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name)
                    .HasJsonPropertyName("name");
                english.Property(e => e.Descripcion)
                    .HasJsonPropertyName("description");
            });
        });

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

        // Unique constraint
        builder.HasIndex(t => t.Code)
            .IsUnique()
            .HasDatabaseName("tenant_statuses_code_key");

        // Check constraint for lowercase code
        builder.ToTable(t => t.HasCheckConstraint("tenant_statuses_code_check", "code = lower(code)"));

        // Indexes
        builder.HasIndex(t => t.Name)
            .HasDatabaseName("idx_tenant_statuses_code");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("idx_tenant_statuses_is_active");

        // Relationships
        builder.HasMany(t => t.Tenants)
            .WithOne(te => te.Status)
            .HasForeignKey(te => te.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
