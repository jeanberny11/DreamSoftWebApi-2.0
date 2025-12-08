using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("countries");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.IsoCode2)
            .HasColumnName("iso_code_2")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(c => c.IsoCode3)
            .HasColumnName("iso_code_3")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.PhoneCode)
            .HasColumnName("phone_code")
            .HasMaxLength(10);

        // TranslatedString as JSONB
        builder.OwnsOne(c => c.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(c => c.IsoCode2).IsUnique().HasDatabaseName("countries_iso_code_2_key");
        builder.HasIndex(c => c.IsoCode3).IsUnique().HasDatabaseName("countries_iso_code_3_key");
        builder.HasIndex(c => c.IsActive).HasDatabaseName("idx_countries_is_active");

        // Relationships
        builder.HasMany(c => c.Provinces)
            .WithOne(p => p.Country)
            .HasForeignKey(p => p.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
