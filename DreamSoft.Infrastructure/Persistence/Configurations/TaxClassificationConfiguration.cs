using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class TaxClassificationConfiguration : IEntityTypeConfiguration<TaxClassification>
{
    public void Configure(EntityTypeBuilder<TaxClassification> builder)
    {
        builder.ToTable("tax_classifications");

        builder.HasKey(tc => tc.Id);

        builder.Property(tc => tc.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(tc => tc.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(tc => tc.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(tc => tc.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(tc => tc.NcfType)
            .HasColumnName("ncf_type")
            .HasMaxLength(50);

        builder.Property(tc => tc.RequiresRnc)
            .HasColumnName("requires_rnc")
            .HasDefaultValue(false);

        // TranslatedString as JSONB (nullable)
        builder.OwnsOne(tc => tc.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(tc => tc.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(tc => tc.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(tc => tc.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(tc => tc.Code).IsUnique().HasDatabaseName("tax_classifications_code_key");
    }
}
