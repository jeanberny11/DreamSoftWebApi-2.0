using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class TenantRegistrationTokenConfiguration
    : IEntityTypeConfiguration<TenantRegistrationToken>
{
    public void Configure(EntityTypeBuilder<TenantRegistrationToken> builder)
    {
        builder.ToTable("tenant_registration_tokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(t => t.CodeHash)
            .HasColumnName("code_hash")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(t => t.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(t => t.AttemptCount)
            .HasColumnName("attempt_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(t => t.IsConsumed)
            .HasColumnName("is_consumed")
            .HasDefaultValue(false)
            .IsRequired();

        // Audit fields
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        // FK — cascade delete so tokens are removed with the tenant
        builder.HasOne(t => t.Tenant)
            .WithMany()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.TenantId)
            .HasDatabaseName("idx_registration_tokens_tenant_id");
    }
}
