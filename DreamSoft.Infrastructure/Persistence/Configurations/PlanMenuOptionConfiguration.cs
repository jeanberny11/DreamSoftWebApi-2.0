using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class PlanMenuOptionConfiguration : IEntityTypeConfiguration<PlanMenuOption>
{
    public void Configure(EntityTypeBuilder<PlanMenuOption> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("plan_menu_options");

        // ── Primary Key (composite) ───────────────────────────────────────
        builder.HasKey(pm => new { pm.PlanId, pm.MenuOptionId });

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(pm => pm.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(pm => pm.MenuOptionId)
            .HasColumnName("menu_option_id")
            .IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(pm => pm.PlanId)
            .HasDatabaseName("idx_plan_menu_options_plan_id");

        builder.HasIndex(pm => pm.MenuOptionId)
            .HasDatabaseName("idx_plan_menu_options_menu_option_id");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(pm => pm.Plan)
            .WithMany(sp => sp.PlanMenuOptions)
            .HasForeignKey(pm => pm.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pm => pm.MenuOption)
            .WithMany(m => m.PlanMenuOptions)
            .HasForeignKey(pm => pm.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
