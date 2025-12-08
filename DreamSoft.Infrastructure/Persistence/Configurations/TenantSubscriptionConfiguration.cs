using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("tenant_subscriptions");

        builder.HasKey(ts => ts.Id);

        builder.Property(ts => ts.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(ts => ts.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(ts => ts.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(ts => ts.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ts => ts.CurrentPeriodStart)
            .HasColumnName("current_period_start")
            .IsRequired();

        builder.Property(ts => ts.CurrentPeriodEnd)
            .HasColumnName("current_period_end")
            .IsRequired();

        builder.Property(ts => ts.StripeCustomerId)
            .HasColumnName("stripe_customer_id")
            .HasMaxLength(255);

        builder.Property(ts => ts.StripeSubscriptionId)
            .HasColumnName("stripe_subscription_id")
            .HasMaxLength(255);

        builder.Property(ts => ts.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(ts => ts.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ts => ts.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(ts => ts.TenantId).HasDatabaseName("idx_tenant_subscriptions_tenant");
        builder.HasIndex(ts => ts.Status).HasDatabaseName("idx_tenant_subscriptions_status");

        // Relationships
        builder.HasOne(ts => ts.Tenant)
            .WithMany()
            .HasForeignKey(ts => ts.TenantId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("tenant_subscriptions_tenant_id_fkey");

        builder.HasOne(ts => ts.Plan)
            .WithMany(p => p.TenantSubscriptions)
            .HasForeignKey(ts => ts.PlanId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("tenant_subscriptions_plan_id_fkey");
    }
}
