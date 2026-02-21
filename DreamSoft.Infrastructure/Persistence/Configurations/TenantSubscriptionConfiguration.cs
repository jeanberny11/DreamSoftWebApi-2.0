using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for TenantSubscription entity
/// Maps to the 'tenant_subscriptions' table in PostgreSQL
/// </summary>
public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        // Table mapping
        builder.ToTable("tenant_subscriptions");

        // Primary key
        builder.HasKey(ts => ts.Id);
        builder.Property(ts => ts.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
        builder.Property(ts => ts.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(ts => ts.SolutionId)
            .HasColumnName("solution_id")
            .IsRequired();

        builder.Property(ts => ts.SubscriptionPlanId)
            .HasColumnName("subscription_plan_id")
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

        builder.Property(ts => ts.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        // Audit fields
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

        // Indexes
        builder.HasIndex(ts => ts.TenantId)
            .HasDatabaseName("idx_tenant_subscriptions_tenant_id");

        builder.HasIndex(ts => ts.SolutionId)
            .HasDatabaseName("idx_tenant_subscriptions_solution_id");

        builder.HasIndex(ts => ts.StatusId)
            .HasDatabaseName("idx_tenant_subscriptions_status_id");

        builder.HasIndex(ts => new { ts.TenantId, ts.IsActive })
            .HasDatabaseName("idx_tenant_subscriptions_tenant_active");

        // Relationships
        builder.HasOne(ts => ts.Tenant)
            .WithMany(t => t.TenantSubscriptions)
            .HasForeignKey(ts => ts.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ts => ts.Solution)
            .WithMany()
            .HasForeignKey(ts => ts.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ts => ts.SubscriptionPlan)
            .WithMany(p => p.TenantSubscriptions)
            .HasForeignKey(ts => ts.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ts => ts.Status)
            .WithMany(s => s.TenantSubscriptions)
            .HasForeignKey(ts => ts.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
