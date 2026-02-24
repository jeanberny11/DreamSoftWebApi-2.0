using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the RefreshToken entity.
/// Maps to the 'refresh_tokens' table in PostgreSQL.
/// One row per login session — supports multiple concurrent sessions per user.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(rt => rt.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(rt => rt.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(rt => rt.CreatedByIp)
            .HasColumnName("created_by_ip")
            .HasMaxLength(50);

        builder.Property(rt => rt.DeviceInfo)
            .HasColumnName("device_info")
            .HasMaxLength(500);

        builder.Property(rt => rt.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(rt => rt.RevokedByIp)
            .HasColumnName("revoked_by_ip")
            .HasMaxLength(50);

        // Audit columns
        builder.Property(rt => rt.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(rt => rt.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(rt => rt.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        // Indexes
        builder.HasIndex(rt => rt.Token)
            .HasDatabaseName("ix_refresh_tokens_token")
            .IsUnique();

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("ix_refresh_tokens_user_id");

        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("ix_refresh_tokens_expires_at");

        // Relationship — cascade delete so tokens are cleaned up when a user is deleted
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_refresh_tokens_users");
    }
}
