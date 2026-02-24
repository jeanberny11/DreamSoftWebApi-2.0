using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for MenuOption entity
/// Maps to the 'menu_options' table in PostgreSQL
/// </summary>
public class MenuOptionConfiguration : IEntityTypeConfiguration<MenuOption>
{
    public void Configure(EntityTypeBuilder<MenuOption> builder)
    {
        // Table mapping
        builder.ToTable("menu_options");

        // Primary key
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
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

        // JSONB Translation Configuration
        builder.OwnsOne(m => m.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(s => s.Name)
                    .HasJsonPropertyName("name")
                    .IsRequired();
                spanish.Property(s => s.Descripcion)
                    .HasJsonPropertyName("description");
            });

            translations.OwnsOne(t => t.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name)
                    .HasJsonPropertyName("name");
                english.Property(e => e.Descripcion)
                    .HasJsonPropertyName("description");
            });
        });

        // Audit fields
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

        // Relationships
        builder.HasOne(m => m.Module)
            .WithMany(mo => mo.MenuOptions)
            .HasForeignKey(m => m.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.MenuGroup)
            .WithMany(g => g.MenuOptions)
            .HasForeignKey(m => m.MenuGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.SolutionMenuOptions)
            .WithOne(sm => sm.MenuOption)
            .HasForeignKey(sm => sm.MenuOptionId)
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
