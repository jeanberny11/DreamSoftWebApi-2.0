using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("subscription_payments");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(p => p.SubscriptionInvoiceId)
            .HasColumnName("subscription_invoice_id")
            .IsRequired();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.PaymentDate)
            .HasColumnName("payment_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(p => p.StripePaymentId)
            .HasColumnName("stripe_payment_id")
            .HasMaxLength(100);

        builder.Property(p => p.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500);

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("idx_subscription_payments_tenant_id");

        builder.HasIndex(p => p.SubscriptionInvoiceId)
            .HasDatabaseName("idx_subscription_payments_invoice_id");

        builder.HasIndex(p => p.StripePaymentId)
            .HasDatabaseName("idx_subscription_payments_stripe_payment_id");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.SubscriptionInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
