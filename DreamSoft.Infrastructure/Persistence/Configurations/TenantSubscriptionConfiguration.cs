using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("tenant_subscriptions");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(ts => ts.Id);
        builder.Property(ts => ts.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(ts => ts.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(ts => ts.SolutionId)
            .HasColumnName("solution_id")
            .IsRequired();

        builder.Property(ts => ts.SubscriptionPlanId)
            .HasColumnName("subscription_plan_id")
            .IsRequired();

        builder.Property(ts => ts.PlanPriceId)
            .HasColumnName("plan_price_id")
            .IsRequired();

        builder.Property(ts => ts.StatusId)
            .HasColumnName("status_id")
            .IsRequired();

        builder.Property(ts => ts.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(ts => ts.EndDate)
            .HasColumnName("end_date");

        builder.Property(ts => ts.TrialEndDate)
            .HasColumnName("trial_end_date");

        builder.Property(ts => ts.StripeSubscriptionId)
            .HasColumnName("stripe_subscription_id")
            .HasMaxLength(100);

        builder.Property(ts => ts.StripeSessionId)
            .HasColumnName("stripe_session_id")
            .HasMaxLength(100);

        builder.Property(ts => ts.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(ts => ts.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ts => ts.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ts => ts.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(ts => new { ts.TenantId, ts.SolutionId })
            .IsUnique()
            .HasDatabaseName("tenant_subscriptions_tenant_solution_key");

        builder.HasIndex(ts => ts.TenantId)
            .HasDatabaseName("idx_tenant_subscriptions_tenant_id");

        builder.HasIndex(ts => ts.SolutionId)
            .HasDatabaseName("idx_tenant_subscriptions_solution_id");

        builder.HasIndex(ts => ts.StatusId)
            .HasDatabaseName("idx_tenant_subscriptions_status_id");

        // Unique index on StripeSessionId — one session maps to exactly one subscription
        builder.HasIndex(ts => ts.StripeSessionId)
            .IsUnique()
            .HasFilter("stripe_session_id IS NOT NULL")
            .HasDatabaseName("idx_tenant_subscriptions_stripe_session_id");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(ts => ts.Tenant)
            .WithMany(t => t.TenantSubscriptions)
            .HasForeignKey(ts => ts.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ts => ts.Solution)
            .WithMany(s => s.TenantSubscriptions)
            .HasForeignKey(ts => ts.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ts => ts.SubscriptionPlan)
            .WithMany(p => p.TenantSubscriptions)
            .HasForeignKey(ts => ts.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ts => ts.PlanPrice)
            .WithMany(pp => pp.TenantSubscriptions)
            .HasForeignKey(ts => ts.PlanPriceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ts => ts.Status)
            .WithMany(s => s.TenantSubscriptions)
            .HasForeignKey(ts => ts.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ts => ts.Invoices)
            .WithOne(i => i.TenantSubscription)
            .HasForeignKey(i => i.TenantSubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
