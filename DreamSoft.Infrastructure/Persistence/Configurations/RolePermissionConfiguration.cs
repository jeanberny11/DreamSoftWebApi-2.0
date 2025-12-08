using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(rp => rp.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(rp => rp.PermissionId)
            .HasColumnName("permission_id")
            .IsRequired();

        builder.Property(rp => rp.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(rp => rp.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(rp => rp.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(rp => rp.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(rp => rp.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(rp => rp.RoleId).HasDatabaseName("idx_role_permissions_role");
        builder.HasIndex(rp => rp.PermissionId).HasDatabaseName("idx_role_permissions_permission");
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique()
            .HasDatabaseName("role_permissions_role_id_permission_id_key");

        // Relationships
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_permissions_role_id_fkey");

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_permissions_permission_id_fkey");

        builder.HasOne(rp => rp.CreatedByUser)
            .WithMany()
            .HasForeignKey(rp => rp.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("role_permissions_created_by_fkey");

        builder.HasOne(rp => rp.UpdatedByUser)
            .WithMany()
            .HasForeignKey(rp => rp.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("role_permissions_updated_by_fkey");
    }
}
