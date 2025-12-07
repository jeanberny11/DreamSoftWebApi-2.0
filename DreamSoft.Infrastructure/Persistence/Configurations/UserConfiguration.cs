using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Identity
        builder.Property(u => u.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        // Profile
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

        builder.Property(u => u.GenderId)
            .HasColumnName("gender_id");

        builder.Property(u => u.DateOfBirth)
            .HasColumnName("date_of_birth")
            .HasColumnType("date");

        builder.Property(u => u.IdTypeId)
            .HasColumnName("id_type_id");

        builder.Property(u => u.IdNumber)
            .HasColumnName("id_number")
            .HasMaxLength(50);

        builder.Property(u => u.Address)
            .HasColumnName("address")
            .HasColumnType("text");

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        // Settings
        builder.Property(u => u.LanguageId)
            .HasColumnName("language_id")
            .IsRequired();

        // Status
        builder.Property(u => u.IsAdmin)
            .HasColumnName("is_admin")
            .HasDefaultValue(false);

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.LastPasswordChangeAt)
            .HasColumnName("last_password_change_at");

        // Audit Fields (from BaseEntity)
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Language)
            .WithMany(l => l.Users)
            .HasForeignKey(u => u.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Gender)
            .WithMany(g => g.Users)
            .HasForeignKey(u => u.GenderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.IdType)
            .WithMany(i => i.Users)
            .HasForeignKey(u => u.IdTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(u => new { u.TenantId, u.Username })
            .IsUnique()
            .HasDatabaseName("users_tenant_id_username_key");

        builder.HasIndex(u => u.TenantId)
            .HasDatabaseName("idx_users_tenant");

        builder.HasIndex(u => new { u.TenantId, u.Username })
            .HasDatabaseName("idx_users_username");

        builder.HasIndex(u => new { u.TenantId, u.IsActive })
            .HasDatabaseName("idx_users_is_active");

        builder.HasIndex(u => new { u.TenantId, u.IsAdmin })
            .HasDatabaseName("idx_users_is_admin");

        builder.HasIndex(u => u.GenderId)
            .HasDatabaseName("idx_users_gender");

        builder.HasIndex(u => u.IdTypeId)
            .HasDatabaseName("idx_users_id_type");

        builder.HasIndex(u => u.LanguageId)
            .HasDatabaseName("fki_users_language_id_fkey");
    }
}