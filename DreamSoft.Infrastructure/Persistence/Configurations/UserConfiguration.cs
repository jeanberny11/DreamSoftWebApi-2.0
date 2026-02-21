using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for User entity
/// Maps to the 'users' table in PostgreSQL
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table mapping
        builder.ToTable("users");

        // Primary key
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
        builder.Property(u => u.TenantId)
            .HasColumnName("tenant_id")
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
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.MiddleName)
            .HasColumnName("middle_name")
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.SecondLastName)
            .HasColumnName("second_last_name")
            .HasMaxLength(100);

        builder.Property(u => u.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(u => u.Mobile)
            .HasColumnName("mobile")
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
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        builder.Property(u => u.LanguageId)
            .HasColumnName("language_id")
            .IsRequired();

        builder.Property(u => u.IsEmailVerified)
            .HasColumnName("is_email_verified")
            .HasDefaultValue(false);

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.RefreshToken)
            .HasColumnName("refresh_token")
            .HasMaxLength(500);

        builder.Property(u => u.RefreshTokenExpiryTime)
            .HasColumnName("refresh_token_expiry_time");

        builder.Property(u => u.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(u => u.UpdatedBy)
            .HasColumnName("updated_by");

        // Audit fields
        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships
        builder.HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Gender)
            .WithMany(g => g.Users)
            .HasForeignKey(u => u.GenderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(u => u.IdType)
            .WithMany(i => i.Users)
            .HasForeignKey(u => u.IdTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(u => u.Language)
            .WithMany(l => l.Users)
            .HasForeignKey(u => u.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.CreatedByUser)
            .WithMany(u => u.CreatedUsers)
            .HasForeignKey(u => u.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.UpdatedByUser)
            .WithMany(u => u.UpdatedUsers)
            .HasForeignKey(u => u.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AssignedUserRoles)
            .WithOne(ur => ur.AssignedByUser)
            .HasForeignKey(ur => ur.AssignedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.CreatedRoles)
            .WithOne(r => r.CreatedByUser)
            .HasForeignKey(r => r.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.UpdatedRoles)
            .WithOne(r => r.UpdatedByUser)
            .HasForeignKey(r => r.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
