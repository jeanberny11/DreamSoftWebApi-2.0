using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RoleOptionActionTemplateConfiguration : IEntityTypeConfiguration<RoleOptionActionTemplate>
{
    public void Configure(EntityTypeBuilder<RoleOptionActionTemplate> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("role_option_actions_template");

        // ── Primary Key (composite) ───────────────────────────────────────
        builder.HasKey(r => new { r.RoleTemplateId, r.MenuOptionId, r.ActionId });

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(r => r.RoleTemplateId)
            .HasColumnName("role_template_id")
            .IsRequired();

        builder.Property(r => r.MenuOptionId)
            .HasColumnName("menu_option_id")
            .IsRequired();

        builder.Property(r => r.ActionId)
            .HasColumnName("action_id")
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(r => r.RoleTemplate)
            .WithMany(rt => rt.RoleOptionActionTemplates)
            .HasForeignKey(r => r.RoleTemplateId)
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
