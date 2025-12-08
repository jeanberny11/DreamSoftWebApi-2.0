using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("provinces");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.CountryId)
            .HasColumnName("country_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(10);

        // TranslatedString as JSONB
        builder.OwnsOne(p => p.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

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
        builder.HasIndex(p => new { p.CountryId, p.Name }).IsUnique().HasDatabaseName("provinces_country_id_name_key");
        builder.HasIndex(p => p.CountryId).HasDatabaseName("idx_provinces_country");
        builder.HasIndex(p => p.IsActive).HasDatabaseName("idx_provinces_is_active");

        // Relationships
        builder.HasOne(p => p.Country)
            .WithMany(c => c.Provinces)
            .HasForeignKey(p => p.CountryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("provinces_country_id_fkey");

        builder.HasMany(p => p.Municipalities)
            .WithOne(m => m.Province)
            .HasForeignKey(m => m.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
