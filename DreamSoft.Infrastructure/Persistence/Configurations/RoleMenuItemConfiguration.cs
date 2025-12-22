using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RoleMenuItemConfiguration : IEntityTypeConfiguration<RoleMenuItem>
{
    public void Configure(EntityTypeBuilder<RoleMenuItem> builder)
    {
        builder.ToTable("role_menu_items");

        // Composite primary key
        builder.HasKey(rmi => new { rmi.RoleId, rmi.MenuItemId });

        builder.Property(rmi => rmi.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(rmi => rmi.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired();

        // Indexes
        builder.HasIndex(rmi => rmi.RoleId)
            .HasDatabaseName("idx_role_menu_items_role");

        builder.HasIndex(rmi => rmi.MenuItemId)
            .HasDatabaseName("idx_role_menu_items_menu_item");

        // Relationships
        builder.HasOne(rmi => rmi.Role)
            .WithMany(r => r.RoleMenuItems)
            .HasForeignKey(rmi => rmi.RoleId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_menu_items_role_id_fkey");

        builder.HasOne(rmi => rmi.MenuItem)
            .WithMany()
            .HasForeignKey(rmi => rmi.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_menu_items_menu_item_id_fkey");
    }
}
