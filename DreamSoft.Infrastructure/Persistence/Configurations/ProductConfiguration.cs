using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // TenantEntity fields
        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(p => p.UpdatedBy)
            .HasColumnName("updated_by");

        // Product-specific fields
        builder.Property(p => p.CategoryId)
            .HasColumnName("category_id");

        builder.Property(p => p.ProductTypeId)
            .HasColumnName("product_type")
            .IsRequired();

        builder.Property(p => p.ProductStatusId)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(p => p.ProductName)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(p => p.Sku)
            .HasColumnName("sku")
            .HasMaxLength(100);

        builder.Property(p => p.Barcode)
            .HasColumnName("barcode")
            .HasMaxLength(100);

        builder.Property(p => p.UnitOfMeasure)
            .HasColumnName("unit_of_measure")
            .HasMaxLength(50)
            .HasDefaultValue("unit");

        builder.Property(p => p.CostPrice)
            .HasColumnName("cost_price")
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.SellingPrice)
            .HasColumnName("selling_price")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.WholesalePrice)
            .HasColumnName("wholesale_price")
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.MinPrice)
            .HasColumnName("min_price")
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.TaxRate)
            .HasColumnName("tax_rate")
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(18.00m);

        builder.Property(p => p.TaxIncluded)
            .HasColumnName("tax_included")
            .HasDefaultValue(false);

        builder.Property(p => p.TrackInventory)
            .HasColumnName("track_inventory")
            .HasDefaultValue(true);

        builder.Property(p => p.CurrentStock)
            .HasColumnName("current_stock")
            .HasColumnType("decimal(10,3)")
            .HasDefaultValue(0);

        builder.Property(p => p.MinimumStock)
            .HasColumnName("minimum_stock")
            .HasColumnType("decimal(10,3)")
            .HasDefaultValue(0);

        builder.Property(p => p.MaximumStock)
            .HasColumnName("maximum_stock")
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.ReorderPoint)
            .HasColumnName("reorder_point")
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.ReorderQuantity)
            .HasColumnName("reorder_quantity")
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.Brand)
            .HasColumnName("brand")
            .HasMaxLength(100);

        builder.Property(p => p.Model)
            .HasColumnName("model")
            .HasMaxLength(100);

        builder.Property(p => p.Weight)
            .HasColumnName("weight")
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.Dimensions)
            .HasColumnName("dimensions")
            .HasMaxLength(100);

        builder.Property(p => p.Color)
            .HasColumnName("color")
            .HasMaxLength(50);

        builder.Property(p => p.Size)
            .HasColumnName("size")
            .HasMaxLength(50);

        builder.Property(p => p.WarrantyMonths)
            .HasColumnName("warranty_months");

        builder.Property(p => p.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        // Audit fields
        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(p => p.TenantId).HasDatabaseName("idx_products_tenant");
        builder.HasIndex(p => new { p.TenantId, p.Sku }).IsUnique().HasDatabaseName("products_tenant_id_sku_key");
        builder.HasIndex(p => new { p.TenantId, p.CategoryId }).HasDatabaseName("idx_products_category");
        builder.HasIndex(p => new { p.TenantId, p.ProductTypeId }).HasDatabaseName("idx_products_type");
        builder.HasIndex(p => new { p.TenantId, p.ProductStatusId }).HasDatabaseName("idx_products_status");
        builder.HasIndex(p => p.Barcode).HasDatabaseName("idx_products_barcode");
        builder.HasIndex(p => new { p.TenantId, p.CurrentStock, p.MinimumStock })
            .HasDatabaseName("idx_products_low_stock")
            .HasFilter("track_inventory = true AND current_stock <= minimum_stock");

        // Relationships
        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("products_tenant_id_fkey");

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("products_created_by_fkey");

        builder.HasOne(p => p.UpdatedByUser)
            .WithMany()
            .HasForeignKey(p => p.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("products_updated_by_fkey");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("products_category_id_fkey");

        builder.HasOne(p => p.ProductType)
            .WithMany(pt => pt.Products)
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("products_product_type_fkey");

        builder.HasOne(p => p.Status)
            .WithMany(ps => ps.Products)
            .HasForeignKey(p => p.ProductStatusId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("products_status_fkey");
    }
}
