using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class GenderConfiguration : IEntityTypeConfiguration<Gender>
{
    public void Configure(EntityTypeBuilder<Gender> builder)
    {
        builder.ToTable("genders");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(g => g.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(g => g.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        // Configure TranslatedString as JSONB (nullable)
        builder.OwnsOne(g => g.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.Property(ts => ts.Spanish)
                .HasJsonPropertyName("es")
                .IsRequired();

            translations.Property(ts => ts.English)
                .HasJsonPropertyName("en");
        });

        builder.Property(g => g.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(g => g.Code)
            .IsUnique()
            .HasDatabaseName("genders_code_key");

        builder.HasIndex(g => g.Name)
            .HasDatabaseName("idx_genders_name");
    }
}