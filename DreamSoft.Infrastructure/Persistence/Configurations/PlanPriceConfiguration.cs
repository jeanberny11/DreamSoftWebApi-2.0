using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class PlanPriceConfiguration : IEntityTypeConfiguration<PlanPrice>
{
    public void Configure(EntityTypeBuilder<PlanPrice> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("plan_prices");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(p => p.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(p => p.BillingCycleId)
            .HasColumnName("billing_cycle_id")
            .IsRequired();

        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(10,2)")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.StripePriceId)
            .HasColumnName("stripe_price_id")
            .HasMaxLength(100);

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(p => new { p.PlanId, p.BillingCycleId })
            .IsUnique()
            .HasDatabaseName("plan_prices_plan_billing_cycle_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(p => p.Plan)
            .WithMany(sp => sp.PlanPrices)
            .HasForeignKey(p => p.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.BillingCycle)
            .WithMany(b => b.PlanPrices)
            .HasForeignKey(p => p.BillingCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.TenantSubscriptions)
            .WithOne(ts => ts.PlanPrice)
            .HasForeignKey(ts => ts.PlanPriceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
