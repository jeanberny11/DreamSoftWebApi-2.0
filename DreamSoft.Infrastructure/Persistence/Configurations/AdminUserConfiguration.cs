using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("admin_users");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(a => a.Email)
            .IsUnique();

        builder.Property(a => a.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.RoleCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.LastLoginAt);
        builder.Property(a => a.FailedLoginAttempts).HasDefaultValue(0);
        builder.Property(a => a.LockoutUntil);

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);
        builder.Property(a => a.IsActive).HasDefaultValue(true);

        // Navigation
        builder.HasMany(a => a.RefreshTokens)
            .WithOne(rt => rt.AdminUser)
            .HasForeignKey(rt => rt.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
