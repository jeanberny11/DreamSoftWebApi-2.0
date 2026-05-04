using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class OptionActionConfiguration : IEntityTypeConfiguration<OptionAction>
{
    public void Configure(EntityTypeBuilder<OptionAction> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("option_actions");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(o => o.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(o => o.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(o => o.Translations, translations =>
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
        builder.Property(o => o.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(o => o.Code)
            .IsUnique()
            .HasDatabaseName("option_actions_code_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasMany(o => o.RoleOptionActions)
            .WithOne(r => r.Action)
            .HasForeignKey(r => r.ActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.RoleOptionActionTemplates)
            .WithOne(r => r.Action)
            .HasForeignKey(r => r.ActionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
