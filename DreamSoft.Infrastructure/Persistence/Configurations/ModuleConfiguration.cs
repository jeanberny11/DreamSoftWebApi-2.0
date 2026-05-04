using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("modules");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(m => m.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(m => m.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(m => m.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(m => m.Icon)
            .HasColumnName("icon")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(m => m.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired()
            .HasDefaultValue(0);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(m => m.Translations, translations =>
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
        builder.Property(m => m.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(m => m.Code)
            .IsUnique()
            .HasDatabaseName("modules_code_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasMany(m => m.MenuOptions)
            .WithOne(o => o.Module)
            .HasForeignKey(o => o.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
