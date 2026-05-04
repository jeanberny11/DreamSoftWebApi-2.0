using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class AdminRefreshTokenConfiguration : IEntityTypeConfiguration<AdminRefreshToken>
{
    public void Configure(EntityTypeBuilder<AdminRefreshToken> builder)
    {
        builder.ToTable("admin_refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.CreatedByIp).HasMaxLength(50);
        builder.Property(rt => rt.DeviceInfo).HasMaxLength(255);
        builder.Property(rt => rt.RevokedAt);
        builder.Property(rt => rt.RevokedByIp).HasMaxLength(50);

        builder.Property(rt => rt.CreatedAt).IsRequired();
        builder.Property(rt => rt.UpdatedAt);
        builder.Property(rt => rt.IsActive).HasDefaultValue(true);

        // FK defined on AdminUserConfiguration side via HasMany/WithOne
    }
}
