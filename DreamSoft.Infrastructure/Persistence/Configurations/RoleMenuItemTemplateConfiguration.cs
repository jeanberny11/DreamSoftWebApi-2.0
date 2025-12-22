using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RoleMenuItemTemplateConfiguration : IEntityTypeConfiguration<RoleMenuItemTemplate>
{
    public void Configure(EntityTypeBuilder<RoleMenuItemTemplate> builder)
    {
        builder.ToTable("role_menu_item_templates");

        // Composite primary key
        builder.HasKey(rmit => new { rmit.RoleTemplateId, rmit.MenuItemId });

        builder.Property(rmit => rmit.RoleTemplateId)
            .HasColumnName("role_template_id")
            .IsRequired();

        builder.Property(rmit => rmit.MenuItemId)
            .HasColumnName("menu_item_id")
            .IsRequired();

        // Indexes
        builder.HasIndex(rmit => rmit.RoleTemplateId)
            .HasDatabaseName("idx_role_menu_item_templates_role_template");

        builder.HasIndex(rmit => rmit.MenuItemId)
            .HasDatabaseName("idx_role_menu_item_templates_menu_item");

        // Relationships
        builder.HasOne(rmit => rmit.RoleTemplate)
            .WithMany(rt => rt.RoleMenuItemTemplates)
            .HasForeignKey(rmit => rmit.RoleTemplateId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_menu_item_templates_role_template_id_fkey");

        builder.HasOne(rmit => rmit.MenuItem)
            .WithMany()
            .HasForeignKey(rmit => rmit.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("role_menu_item_templates_menu_item_id_fkey");
    }
}
