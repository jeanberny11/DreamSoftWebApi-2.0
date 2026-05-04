using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class IdTypeConfiguration : IEntityTypeConfiguration<IdType>
{
    public void Configure(EntityTypeBuilder<IdType> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("id_types");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(i => i.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(i => i.CountryId)
            .HasColumnName("country_id")
            .IsRequired();

        builder.Property(i => i.ValidationPattern)
            .HasColumnName("validation_pattern")
            .HasMaxLength(255)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(i => i.Translations, translations =>
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
        builder.Property(i => i.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(i => i.Country)
            .WithMany(c => c.IdTypes)
            .HasForeignKey(i => i.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
