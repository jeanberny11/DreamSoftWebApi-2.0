using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu_items");

        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(mi => mi.ModuleId)
            .HasColumnName("module_id")
            .IsRequired();

        builder.Property(mi => mi.MenuGroupId)
            .HasColumnName("menu_group_id")
            .IsRequired();

        builder.Property(mi => mi.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(mi => mi.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(mi => mi.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(mi => mi.Route)
            .HasColumnName("route")
            .HasMaxLength(255);

        builder.Property(mi => mi.Icon)
            .HasColumnName("icon")
            .HasMaxLength(100);

        builder.Property(mi => mi.RequiredTierId)
            .HasColumnName("required_tier_id");

        builder.Property(mi => mi.SortOrder)
            .HasColumnName("sort_order");

        // TranslatedString as JSONB (nullable)
        builder.OwnsOne(mi => mi.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(mi => mi.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(mi => mi.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(mi => mi.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(mi => mi.Code).IsUnique().HasDatabaseName("menu_items_code_key");
        builder.HasIndex(mi => mi.Code).HasDatabaseName("idx_menu_items_code");
        builder.HasIndex(mi => mi.ModuleId).HasDatabaseName("idx_menu_items_module");
        builder.HasIndex(mi => mi.MenuGroupId).HasDatabaseName("idx_menu_items_menu_group");
        builder.HasIndex(mi => mi.Translations).HasDatabaseName("idx_menu_items_translations");

        // Relationships
        builder.HasOne(mi => mi.Module)
            .WithMany(m => m.MenuItems)
            .HasForeignKey(mi => mi.ModuleId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("menu_items_module_id_fkey");

        builder.HasOne(mi => mi.MenuGroup)
            .WithMany(mg => mg.MenuItems)
            .HasForeignKey(mi => mi.MenuGroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("menu_items_menu_group_id_fkey");

        builder.HasOne(mi => mi.RequiredTier)
            .WithMany()
            .HasForeignKey(mi => mi.RequiredTierId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("menu_items_required_tier_id_fkey");
    }
}
