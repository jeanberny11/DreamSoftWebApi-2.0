using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("product_categories");

        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // TenantEntity fields
        builder.Property(pc => pc.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(pc => pc.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(pc => pc.UpdatedBy)
            .HasColumnName("updated_by");

        // ProductCategory-specific fields
        builder.Property(pc => pc.ParentId)
            .HasColumnName("parent_id");

        builder.Property(pc => pc.CategoryName)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(pc => pc.Code)
            .HasColumnName("code")
            .HasMaxLength(50);

        builder.Property(pc => pc.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        // Audit fields
        builder.Property(pc => pc.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(pc => pc.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pc => pc.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(pc => pc.TenantId).HasDatabaseName("idx_product_categories_tenant");
        builder.HasIndex(pc => pc.ParentId).HasDatabaseName("idx_product_categories_parent");
        builder.HasIndex(pc => new { pc.TenantId, pc.CategoryName })
            .IsUnique()
            .HasDatabaseName("product_categories_tenant_id_name_key");

        // Relationships
        builder.HasOne(pc => pc.Tenant)
            .WithMany()
            .HasForeignKey(pc => pc.TenantId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_categories_tenant_id_fkey");

        builder.HasOne(pc => pc.CreatedByUser)
            .WithMany()
            .HasForeignKey(pc => pc.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("product_categories_created_by_fkey");

        builder.HasOne(pc => pc.UpdatedByUser)
            .WithMany()
            .HasForeignKey(pc => pc.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("product_categories_updated_by_fkey");

        builder.HasOne(pc => pc.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(pc => pc.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("product_categories_parent_id_fkey");
    }
}
