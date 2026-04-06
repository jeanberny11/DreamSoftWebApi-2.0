using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("users");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(u => u.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(u => u.SolutionId)
            .HasColumnName("solution_id")
            .IsRequired();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        builder.Property(u => u.LanguageId)
            .HasColumnName("language_id");

        builder.Property(u => u.GenderId)
            .HasColumnName("gender_id");

        builder.Property(u => u.IsAdmin)
            .HasColumnName("is_admin")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.RoleId)
            .HasColumnName("role_id");

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.LockoutUntil)
            .HasColumnName("lockout_until");

        builder.Property(u => u.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(u => u.UpdatedBy)
            .HasColumnName("updated_by");

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(u => new { u.TenantId, u.SolutionId, u.Username })
            .IsUnique()
            .HasDatabaseName("users_tenant_solution_username_key");

        builder.HasIndex(u => new { u.TenantId, u.SolutionId, u.Email })
            .IsUnique()
            .HasDatabaseName("users_tenant_solution_email_key");

        builder.HasIndex(u => new { u.TenantId, u.SolutionId })
            .HasDatabaseName("idx_users_tenant_solution");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(u => u.Gender)
            .WithMany(g => g.Users)
            .HasForeignKey(u => u.GenderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(u => u.Language)
            .WithMany(l => l.Users)
            .HasForeignKey(u => u.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
