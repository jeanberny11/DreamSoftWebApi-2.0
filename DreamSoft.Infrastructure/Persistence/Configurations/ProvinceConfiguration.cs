using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Province entity
/// Maps to the 'provinces' table in PostgreSQL
/// </summary>
public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        // Table mapping
        builder.ToTable("provinces");

        // Primary key
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
        builder.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(p => p.CountryId)
            .HasColumnName("country_id")
            .IsRequired();

        // JSONB Translation Configuration (Name Only - No Description)
        builder.OwnsOne(p => p.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(s => s.Name)
                    .HasJsonPropertyName("name")
                    .IsRequired();
                spanish.Ignore(s => s.Descripcion);
            });

            translations.OwnsOne(t => t.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name)
                    .HasJsonPropertyName("name");
                english.Ignore(e => e.Descripcion);
            });
        });

        // Audit fields
        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships
        builder.HasOne(p => p.Country)
            .WithMany(c => c.Provinces)
            .HasForeignKey(p => p.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Municipalities)
            .WithOne(m => m.Province)
            .HasForeignKey(m => m.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Tenants)
            .WithOne(t => t.Province)
            .HasForeignKey(t => t.ProvinceId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
