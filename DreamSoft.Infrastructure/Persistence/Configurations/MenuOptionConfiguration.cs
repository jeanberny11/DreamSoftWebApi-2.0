using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class MenuOptionConfiguration : IEntityTypeConfiguration<MenuOption>
{
    public void Configure(EntityTypeBuilder<MenuOption> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("menu_options");

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
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(m => m.ModuleId)
            .HasColumnName("module_id")
            .IsRequired();

        builder.Property(m => m.MenuGroupId)
            .HasColumnName("menu_group_id")
            .IsRequired();

        builder.Property(m => m.Route)
            .HasColumnName("route")
            .HasMaxLength(255)
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
            .HasDatabaseName("menu_options_code_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(m => m.Module)
            .WithMany(mo => mo.MenuOptions)
            .HasForeignKey(m => m.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.MenuGroup)
            .WithMany(g => g.MenuOptions)
            .HasForeignKey(m => m.MenuGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.PlanMenuOptions)
            .WithOne(pm => pm.MenuOption)
            .HasForeignKey(pm => pm.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.RoleMenuOptions)
            .WithOne(r => r.MenuOption)
            .HasForeignKey(r => r.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.RoleMenuOptionTemplates)
            .WithOne(r => r.MenuOption)
            .HasForeignKey(r => r.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.RoleOptionActions)
            .WithOne(r => r.MenuOption)
            .HasForeignKey(r => r.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.RoleOptionActionTemplates)
            .WithOne(r => r.MenuOption)
            .HasForeignKey(r => r.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
