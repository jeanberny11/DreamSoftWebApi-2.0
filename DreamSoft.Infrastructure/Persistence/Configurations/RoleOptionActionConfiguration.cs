using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RoleOptionActionConfiguration : IEntityTypeConfiguration<RoleOptionAction>
{
    public void Configure(EntityTypeBuilder<RoleOptionAction> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("role_option_actions");

        // ── Primary Key (composite) ───────────────────────────────────────
        builder.HasKey(r => new { r.RoleId, r.MenuOptionId, r.ActionId });

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(r => r.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(r => r.MenuOptionId)
            .HasColumnName("menu_option_id")
            .IsRequired();

        builder.Property(r => r.ActionId)
            .HasColumnName("action_id")
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(r => r.Role)
            .WithMany(ro => ro.RoleOptionActions)
            .HasForeignKey(r => r.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.MenuOption)
            .WithMany(m => m.RoleOptionActions)
            .HasForeignKey(r => r.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Action)
            .WithMany(a => a.RoleOptionActions)
            .HasForeignKey(r => r.ActionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
