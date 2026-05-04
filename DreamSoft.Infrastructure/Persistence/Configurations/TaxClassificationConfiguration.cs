using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class TaxClassificationConfiguration : IEntityTypeConfiguration<TaxClassification>
{
    public void Configure(EntityTypeBuilder<TaxClassification> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("tax_classifications");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(tc => tc.Id);
        builder.Property(tc => tc.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(tc => tc.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(tc => tc.Code)
            .IsUnique()
            .HasDatabaseName("tax_classifications_code_key");

        builder.Property(tc => tc.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(tc => tc.NcfType)
            .HasColumnName("ncf_type")
            .HasMaxLength(50);

        builder.Property(tc => tc.RequiresRnc)
            .HasColumnName("requires_rnc")
            .IsRequired()
            .HasDefaultValue(false);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(tc => tc.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(s => s.Name).HasJsonPropertyName("name").IsRequired();
                spanish.Property(s => s.Descripcion).HasJsonPropertyName("description");
            });

            translations.OwnsOne(t => t.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name).HasJsonPropertyName("name");
                english.Property(e => e.Descripcion).HasJsonPropertyName("description");
            });
        });

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(tc => tc.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(tc => tc.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(tc => tc.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(tc => tc.IsActive)
            .HasDatabaseName("idx_tax_classifications_active");

        builder.HasIndex(tc => tc.Code)
            .HasDatabaseName("idx_tax_classifications_code");
    }
}
