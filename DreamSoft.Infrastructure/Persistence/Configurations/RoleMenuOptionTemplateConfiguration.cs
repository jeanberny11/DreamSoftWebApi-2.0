using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RoleMenuOptionTemplateConfiguration : IEntityTypeConfiguration<RoleMenuOptionTemplate>
{
    public void Configure(EntityTypeBuilder<RoleMenuOptionTemplate> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("role_menu_options_template");

        // ── Primary Key (composite) ───────────────────────────────────────
        builder.HasKey(r => new { r.RoleTemplateId, r.MenuOptionId });

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(r => r.RoleTemplateId)
            .HasColumnName("role_template_id")
            .IsRequired();

        builder.Property(r => r.MenuOptionId)
            .HasColumnName("menu_option_id")
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(r => r.RoleTemplate)
            .WithMany(rt => rt.RoleMenuOptionTemplates)
            .HasForeignKey(r => r.RoleTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.MenuOption)
            .WithMany(m => m.RoleMenuOptionTemplates)
            .HasForeignKey(r => r.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
