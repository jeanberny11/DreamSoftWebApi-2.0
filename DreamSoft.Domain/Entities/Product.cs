using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class Product : TenantEntity
{
    public int? CategoryId { get; protected set; }
    public int ProductTypeId { get; protected set; }
    public int ProductStatusId { get; protected set; }

    // Basic info
    public string ProductName { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public string? Sku { get; protected set; }
    public string? Barcode { get; protected set; }
    public string UnitOfMeasure { get; protected set; } = "unit";

    // Pricing
    public decimal? CostPrice { get; protected set; }
    public decimal SellingPrice { get; protected set; }
    public decimal? WholesalePrice { get; protected set; }
    public decimal? MinPrice { get; protected set; }

    // Tax
    public decimal TaxRate { get; protected set; } = 18.00m;
    public bool TaxIncluded { get; protected set; }

    // Inventory
    public bool TrackInventory { get; protected set; } = true;
    public decimal CurrentStock { get; protected set; }
    public decimal MinimumStock { get; protected set; }
    public decimal? MaximumStock { get; protected set; }
    public decimal? ReorderPoint { get; protected set; }
    public decimal? ReorderQuantity { get; protected set; }

    // Additional details
    public string? Brand { get; protected set; }
    public string? Model { get; protected set; }
    public decimal? Weight { get; protected set; }
    public string? Dimensions { get; protected set; }
    public string? Color { get; protected set; }
    public string? Size { get; protected set; }
    public int? WarrantyMonths { get; protected set; }
    public string? Notes { get; protected set; }

    // Navigation properties
    public ProductCategory? Category { get; private set; }
    public ProductType ProductType { get; private set; } = null!;
    public ProductStatus Status { get; private set; } = null!;
    public ICollection<ProductImage> Images { get; private set; } = [];

    private Product() { }

    public static Product Create(int tenantId, int productTypeId, int productStatusId, string productName, decimal sellingPrice, int? createdBy, int? categoryId = null)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required", nameof(productName));

        if (productTypeId <= 0)
            throw new ArgumentException("Product type ID is required", nameof(productTypeId));

        if (productStatusId <= 0)
            throw new ArgumentException("Product status ID is required", nameof(productStatusId));

        if (sellingPrice < 0)
            throw new ArgumentException("Selling price must be non-negative", nameof(sellingPrice));

        var product = new Product
        {
            CategoryId = categoryId,
            ProductTypeId = productTypeId,
            ProductStatusId = productStatusId,
            ProductName = productName.Trim(),
            SellingPrice = sellingPrice,
            CurrentStock = 0,
            MinimumStock = 0
        };

        product.InitializeTenantEntity(tenantId, createdBy);
        return product;
    }
}
