using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class SubscriptionCancellationLogConfiguration : IEntityTypeConfiguration<SubscriptionCancellationLog>
{
    public void Configure(EntityTypeBuilder<SubscriptionCancellationLog> builder)
    {
        // ── Table ──────────────────────────────────────────────────────────────
        builder.ToTable("subscription_cancellation_logs");

        // ── Primary Key ────────────────────────────────────────────────────────
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ─────────────────────────────────────────────────────────
        builder.Property(l => l.TenantSubscriptionId)
            .HasColumnName("tenant_subscription_id")
            .IsRequired();

        builder.Property(l => l.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(l => l.CancelledAt)
            .HasColumnName("cancelled_at")
            .IsRequired();

        builder.Property(l => l.CancellationType)
            .HasColumnName("cancellation_type")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(100);

        builder.Property(l => l.CancellationFeedback)
            .HasColumnName("cancellation_feedback")
            .HasColumnType("text");

        builder.Property(l => l.ScheduledEndDate)
            .HasColumnName("scheduled_end_date");

        // ── Audit Fields ───────────────────────────────────────────────────────
        builder.Property(l => l.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ────────────────────────────────────────────────────────────
        builder.HasIndex(l => l.TenantSubscriptionId)
            .HasDatabaseName("idx_subscription_cancellation_logs_subscription_id");

        builder.HasIndex(l => l.TenantId)
            .HasDatabaseName("idx_subscription_cancellation_logs_tenant_id");

        // ── Relationships ──────────────────────────────────────────────────────
        builder.HasOne(l => l.TenantSubscription)
            .WithMany(ts => ts.CancellationLogs)
            .HasForeignKey(l => l.TenantSubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Tenant)
            .WithMany()
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
