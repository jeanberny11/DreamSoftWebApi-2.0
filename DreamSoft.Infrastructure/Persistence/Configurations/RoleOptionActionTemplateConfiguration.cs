using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for RoleOptionActionTemplate entity
/// Maps to the 'role_option_actions_template' table in PostgreSQL
/// </summary>
public class RoleOptionActionTemplateConfiguration : IEntityTypeConfiguration<RoleOptionActionTemplate>
{
    public void Configure(EntityTypeBuilder<RoleOptionActionTemplate> builder)
    {
        // Table mapping
        builder.ToTable("role_option_actions_template");

        // Composite primary key
        builder.HasKey(r => new { r.RoleId, r.MenuOptionId, r.ActionId });

        // Properties mapping
        builder.Property(r => r.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(r => r.MenuOptionId)
            .HasColumnName("menu_option_id")
            .IsRequired();

        builder.Property(r => r.ActionId)
            .HasColumnName("action_id")
            .IsRequired();

        // Relationships
        builder.HasOne(r => r.Role)
            .WithMany(rt => rt.RoleOptionActionTemplates)
            .HasForeignKey(r => r.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.MenuOption)
            .WithMany(m => m.RoleOptionActionTemplates)
            .HasForeignKey(r => r.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Action)
            .WithMany(a => a.RoleOptionActionTemplates)
            .HasForeignKey(r => r.ActionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
