using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(ur => ur.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(ur => ur.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(ur => ur.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(ur => ur.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(ur => ur.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ur => ur.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(ur => ur.UserId).HasDatabaseName("idx_user_roles_user");
        builder.HasIndex(ur => ur.RoleId).HasDatabaseName("idx_user_roles_role");
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasDatabaseName("user_roles_user_id_role_id_key");

        // Relationships
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("user_roles_user_id_fkey");

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("user_roles_role_id_fkey");

        builder.HasOne(ur => ur.CreatedByUser)
            .WithMany()
            .HasForeignKey(ur => ur.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("user_roles_created_by_fkey");

        builder.HasOne(ur => ur.UpdatedByUser)
            .WithMany()
            .HasForeignKey(ur => ur.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("user_roles_updated_by_fkey");
    }
}
