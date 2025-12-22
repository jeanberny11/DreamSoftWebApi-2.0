using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RoleMenuItemActionConfiguration : IEntityTypeConfiguration<RoleMenuItemAction>
{
    public void Configure(EntityTypeBuilder<RoleMenuItemAction> builder)
    {
        builder.ToTable("role_menu_item_actions");

        // Composite primary key
        builder.HasKey(rmia => new { rmia.RoleId, rmia.MenuItemId, rmia.ActionId });

        builder.Property(rmia => rmia.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(rmia => rmia.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired();

        builder.Property(rmia => rmia.ActionId)
            .HasColumnName("action_id")
            .IsRequired();

        // Indexes
        builder.HasIndex(rmia => rmia.RoleId)
            .HasDatabaseName("idx_role_menu_item_actions_role");

        builder.HasIndex(rmia => rmia.MenuItemId)
            .HasDatabaseName("idx_role_menu_item_actions_menu_item");

        builder.HasIndex(rmia => rmia.ActionId)
            .HasDatabaseName("idx_role_menu_item_actions_action");

        builder.HasIndex(rmia => new { rmia.RoleId, rmia.MenuItemId })
            .HasDatabaseName("idx_role_menu_item_actions_role_menu");

        // Relationships
        builder.HasOne(rmia => rmia.Role)
            .WithMany(r => r.RoleMenuItemActions)
            .HasForeignKey(rmia => rmia.RoleId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_menu_item_actions_role_id_fkey");

        builder.HasOne(rmia => rmia.MenuItem)
            .WithMany()
            .HasForeignKey(rmia => rmia.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_menu_item_actions_menu_item_id_fkey");

        builder.HasOne(rmia => rmia.PermissionAction)
            .WithMany(pa => pa.RoleMenuItemActions)
            .HasForeignKey(rmia => rmia.ActionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_menu_item_actions_action_id_fkey");
    }
}
