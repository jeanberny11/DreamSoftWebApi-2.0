using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired();

        builder.Property(p => p.ActionId)
            .HasColumnName("action")
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        // TranslatedString as JSONB (nullable)
        builder.OwnsOne(p => p.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(p => new { p.MenuItemId, p.ActionId })
            .IsUnique()
            .HasDatabaseName("permissions_menu_item_id_action_key");

        builder.HasIndex(p => p.MenuItemId).HasDatabaseName("idx_permissions_menu_item");
        builder.HasIndex(p => p.ActionId).HasDatabaseName("idx_permissions_action");

        // Relationships
        builder.HasOne(p => p.MenuItem)
            .WithMany(mi => mi.Permissions)
            .HasForeignKey(p => p.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("permissions_menu_item_id_fkey");

        builder.HasOne(p => p.Action)
            .WithMany(pa => pa.Permissions)
            .HasForeignKey(p => p.ActionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("permissions_action_fkey");
    }
}
