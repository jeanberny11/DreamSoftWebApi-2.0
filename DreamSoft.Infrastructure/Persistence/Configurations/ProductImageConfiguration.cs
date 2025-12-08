using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // TenantEntity fields
        builder.Property(pi => pi.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(pi => pi.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(pi => pi.UpdatedBy)
            .HasColumnName("updated_by");

        // ProductImage-specific fields
        builder.Property(pi => pi.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(pi => pi.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(pi => pi.AltText)
            .HasColumnName("alt_text")
            .HasMaxLength(255);

        builder.Property(pi => pi.IsPrimary)
            .HasColumnName("is_primary")
            .HasDefaultValue(false);

        builder.Property(pi => pi.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        // Audit fields
        builder.Property(pi => pi.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(pi => pi.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pi => pi.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(pi => pi.TenantId).HasDatabaseName("idx_product_images_tenant");
        builder.HasIndex(pi => pi.ProductId).HasDatabaseName("idx_product_images_product");
        builder.HasIndex(pi => new { pi.ProductId, pi.IsPrimary }).HasDatabaseName("idx_product_images_primary");
        builder.HasIndex(pi => pi.ProductId)
            .IsUnique()
            .HasDatabaseName("idx_product_images_one_primary")
            .HasFilter("is_primary = true");

        // Relationships
        builder.HasOne(pi => pi.Tenant)
            .WithMany()
            .HasForeignKey(pi => pi.TenantId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_images_tenant_id_fkey");

        builder.HasOne(pi => pi.CreatedByUser)
            .WithMany()
            .HasForeignKey(pi => pi.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("product_images_created_by_fkey");

        builder.HasOne(pi => pi.UpdatedByUser)
            .WithMany()
            .HasForeignKey(pi => pi.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("product_images_updated_by_fkey");

        builder.HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_images_product_id_fkey");
    }
}
