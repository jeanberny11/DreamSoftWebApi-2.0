using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("countries");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(c => c.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(c => c.IsoCode)
            .HasColumnName("iso_code")
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(c => c.PhoneCode)
            .HasColumnName("phone_code")
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(c => c.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(s => s.Name).HasJsonPropertyName("name").IsRequired();
                spanish.Ignore(s => s.Descripcion);
            });

            translations.OwnsOne(t => t.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name).HasJsonPropertyName("name");
                english.Ignore(e => e.Descripcion);
            });
        });

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("countries_code_key");

        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("countries_name_key");

        builder.HasIndex(c => c.IsoCode)
            .IsUnique()
            .HasDatabaseName("countries_iso_code_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasMany(c => c.Provinces)
            .WithOne(p => p.Country)
            .HasForeignKey(p => p.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Tenants)
            .WithOne(t => t.Country)
            .HasForeignKey(t => t.CountryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(c => c.IdTypes)
            .WithOne(i => i.Country)
            .HasForeignKey(i => i.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
