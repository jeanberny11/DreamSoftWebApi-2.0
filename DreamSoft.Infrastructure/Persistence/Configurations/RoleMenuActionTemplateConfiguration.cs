using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RoleMenuActionTemplateConfiguration : IEntityTypeConfiguration<RoleMenuActionTemplate>
{
    public void Configure(EntityTypeBuilder<RoleMenuActionTemplate> builder)
    {
        builder.ToTable("role_menu_action_templates");

        // Composite primary key
        builder.HasKey(rmat => new { rmat.RoleTemplateId, rmat.MenuItemId, rmat.ActionId });

        builder.Property(rmat => rmat.RoleTemplateId)
            .HasColumnName("role_template_id")
            .IsRequired();

        builder.Property(rmat => rmat.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired();

        builder.Property(rmat => rmat.ActionId)
            .HasColumnName("action_id")
            .IsRequired();

        // Indexes
        builder.HasIndex(rmat => rmat.RoleTemplateId)
            .HasDatabaseName("idx_role_menu_action_templates_role_template");

        builder.HasIndex(rmat => rmat.MenuItemId)
            .HasDatabaseName("idx_role_menu_action_templates_menu_item");

        builder.HasIndex(rmat => rmat.ActionId)
            .HasDatabaseName("idx_role_menu_action_templates_action");

        builder.HasIndex(rmat => new { rmat.RoleTemplateId, rmat.MenuItemId })
            .HasDatabaseName("idx_role_menu_action_templates_role_menu");

        // Relationships
        builder.HasOne(rmat => rmat.RoleTemplate)
            .WithMany(rt => rt.RoleMenuActionTemplates)
            .HasForeignKey(rmat => rmat.RoleTemplateId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_menu_action_templates_role_template_id_fkey");

        builder.HasOne(rmat => rmat.MenuItem)
            .WithMany()
            .HasForeignKey(rmat => rmat.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_menu_action_templates_menu_item_id_fkey");

        builder.HasOne(rmat => rmat.PermissionAction)
            .WithMany(pa => pa.RoleMenuActionTemplates)
            .HasForeignKey(rmat => rmat.ActionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_menu_action_templates_action_id_fkey");
    }
}
