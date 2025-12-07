using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class ProductImage : TenantEntity
{
    public int ProductId { get; protected set; }
    public string ImageUrl { get; protected set; } = null!;
    public string? AltText { get; protected set; }
    public bool IsPrimary { get; protected set; }
    public int SortOrder { get; protected set; }

    // Navigation properties
    public Product Product { get; private set; } = null!;

    private ProductImage() { }

    public static ProductImage Create(int tenantId, int productId, string imageUrl, int? createdBy, string? altText = null, bool isPrimary = false, int sortOrder = 0)
    {
        if (productId <= 0)
            throw new ArgumentException("Product ID is required", nameof(productId));

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is required", nameof(imageUrl));

        var productImage = new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl.Trim(),
            AltText = altText?.Trim(),
            IsPrimary = isPrimary,
            SortOrder = sortOrder
        };

        productImage.InitializeTenantEntity(tenantId, createdBy);
        return productImage;
    }
}
