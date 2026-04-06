using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("languages");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(l => l.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(l => l.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(l => l.Translations, translations =>
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
        builder.Property(l => l.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(l => l.Code)
            .IsUnique()
            .HasDatabaseName("languages_code_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasMany(l => l.Tenants)
            .WithOne(t => t.Language)
            .HasForeignKey(t => t.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Users)
            .WithOne(u => u.Language)
            .HasForeignKey(u => u.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
