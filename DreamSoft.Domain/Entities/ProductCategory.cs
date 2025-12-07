using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class ProductCategory : TenantEntity
{
    public int? ParentId { get; protected set; }
    public string CategoryName { get; protected set; } = null!;
    public string? Code { get; protected set; }
    public string? Description { get; protected set; }

    // Navigation properties
    public ProductCategory? Parent { get; private set; }
    public ICollection<ProductCategory> Children { get; private set; } = [];
    public ICollection<Product> Products { get; private set; } = [];

    private ProductCategory() { }

    public static ProductCategory Create(int tenantId, string categoryName, int? createdBy, int? parentId = null, string? code = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new ArgumentException("Category name is required", nameof(categoryName));

        var category = new ProductCategory
        {
            ParentId = parentId,
            CategoryName = categoryName.Trim(),
            Code = code?.ToUpper().Trim(),
            Description = description?.Trim()
        };

        category.InitializeTenantEntity(tenantId, createdBy);
        return category;
    }
}
