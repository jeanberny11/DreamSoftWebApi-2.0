using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.ToTable("product_types");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(pt => pt.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        // TranslatedString as JSONB
        builder.OwnsOne(pt => pt.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(pt => pt.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(pt => pt.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(pt => pt.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(pt => pt.Name).IsUnique().HasDatabaseName("product_types_name_key");
    }
}
