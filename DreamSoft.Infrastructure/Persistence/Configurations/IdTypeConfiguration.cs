using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class IdTypeConfiguration : IEntityTypeConfiguration<IdType>
{
    public void Configure(EntityTypeBuilder<IdType> builder)
    {
        builder.ToTable("id_types");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.CountryId)
            .HasColumnName("country_id")
            .IsRequired();

        builder.Property(i => i.Code)
            .HasColumnName("code")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(i => i.ValidationPattern)
            .HasColumnName("validation_pattern")
            .HasMaxLength(255);

        // Configure TranslatedString as JSONB (nullable)
        builder.OwnsOne(i => i.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.Property(ts => ts.Spanish)
                .HasJsonPropertyName("es")
                .IsRequired();

            translations.Property(ts => ts.English)
                .HasJsonPropertyName("en");
        });

        builder.Property(i => i.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(i => new { i.CountryId, i.Code })
            .IsUnique()
            .HasDatabaseName("id_types_country_id_code_key");

        builder.HasIndex(i => i.CountryId)
            .HasDatabaseName("idx_id_types_country");

        builder.HasIndex(i => i.IsActive)
            .HasDatabaseName("idx_id_types_is_active");
    }
}