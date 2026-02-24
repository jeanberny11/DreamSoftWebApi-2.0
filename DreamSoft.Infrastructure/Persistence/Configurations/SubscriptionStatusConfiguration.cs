using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for SubscriptionStatus entity
/// Maps to the 'subscription_statuses' table in PostgreSQL
/// </summary>
public class SubscriptionStatusConfiguration : IEntityTypeConfiguration<SubscriptionStatus>
{
    public void Configure(EntityTypeBuilder<SubscriptionStatus> builder)
    {
        // Table mapping
        builder.ToTable("subscription_statuses");

        // Primary key
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
        builder.Property(s => s.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        // JSONB Translation Configuration
        builder.OwnsOne(s => s.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(sp => sp.Name)
                    .HasJsonPropertyName("name")
                    .IsRequired();
                spanish.Property(sp => sp.Descripcion)
                    .HasJsonPropertyName("description");
            });

            translations.OwnsOne(t => t.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name)
                    .HasJsonPropertyName("name");
                english.Property(e => e.Descripcion)
                    .HasJsonPropertyName("description");
            });
        });

        // Audit fields
        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        // Unique constraints
        builder.HasIndex(s => s.Code)
            .IsUnique()
            .HasDatabaseName("subscription_statuses_code_key");

        // Indexes
        builder.HasIndex(s => s.Code)
            .HasDatabaseName("idx_subscription_statuses_code");

        // Relationships
        builder.HasMany(s => s.TenantSubscriptions)
            .WithOne(ts => ts.Status)
            .HasForeignKey(ts => ts.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
