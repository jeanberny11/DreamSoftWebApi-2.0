using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for RoleMenuOptionTemplate entity
/// Maps to the 'role_menu_options_template' table in PostgreSQL
/// </summary>
public class RoleMenuOptionTemplateConfiguration : IEntityTypeConfiguration<RoleMenuOptionTemplate>
{
    public void Configure(EntityTypeBuilder<RoleMenuOptionTemplate> builder)
    {
        // Table mapping
        builder.ToTable("role_menu_options_template");

        // Composite primary key
        builder.HasKey(r => new { r.RoleId, r.MenuOptionId });

        // Properties mapping
        builder.Property(r => r.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(r => r.MenuOptionId)
            .HasColumnName("menu_option_id")
            .IsRequired();

        // Relationships
        builder.HasOne(r => r.Role)
            .WithMany(rt => rt.RoleMenuOptionTemplates)
            .HasForeignKey(r => r.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.MenuOption)
            .WithMany(m => m.RoleMenuOptionTemplates)
            .HasForeignKey(r => r.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
