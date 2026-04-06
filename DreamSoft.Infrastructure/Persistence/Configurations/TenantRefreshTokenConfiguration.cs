using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class TenantRefreshTokenConfiguration : IEntityTypeConfiguration<TenantRefreshToken>
{
    public void Configure(EntityTypeBuilder<TenantRefreshToken> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("tenant_refresh_tokens");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(t => t.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(t => t.CreatedByIp)
            .HasColumnName("created_by_ip")
            .HasMaxLength(50);

        builder.Property(t => t.DeviceInfo)
            .HasColumnName("device_info")
            .HasMaxLength(500);

        builder.Property(t => t.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.RevokedByIp)
            .HasColumnName("revoked_by_ip")
            .HasMaxLength(50);

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(t => t.Token)
            .IsUnique()
            .HasDatabaseName("ix_tenant_refresh_tokens_token");

        builder.HasIndex(t => t.TenantId)
            .HasDatabaseName("ix_tenant_refresh_tokens_tenant_id");

        builder.HasIndex(t => t.ExpiresAt)
            .HasDatabaseName("ix_tenant_refresh_tokens_expires_at");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(t => t.Tenant)
            .WithMany(te => te.RefreshTokens)
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_tenant_refresh_tokens_tenants");
    }
}
