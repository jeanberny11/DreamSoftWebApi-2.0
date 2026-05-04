using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class SubscriptionInvoiceConfiguration : IEntityTypeConfiguration<SubscriptionInvoice>
{
    public void Configure(EntityTypeBuilder<SubscriptionInvoice> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("subscription_invoices");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(i => i.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(i => i.TenantSubscriptionId)
            .HasColumnName("tenant_subscription_id")
            .IsRequired();

        builder.Property(i => i.Amount)
            .HasColumnName("amount")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(i => i.Currency)
            .HasColumnName("currency")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(i => i.PaidAt)
            .HasColumnName("paid_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(i => i.StripeInvoiceId)
            .HasColumnName("stripe_invoice_id")
            .HasMaxLength(100);

        builder.Property(i => i.StripePaymentIntentId)
            .HasColumnName("stripe_payment_intent_id")
            .HasMaxLength(100);

        builder.Property(i => i.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        builder.Property(i => i.HostedInvoiceUrl)
            .HasColumnName("hosted_invoice_url")
            .HasColumnType("text");

        builder.Property(i => i.InvoicePdfUrl)
            .HasColumnName("invoice_pdf_url")
            .HasColumnType("text");

        builder.Property(i => i.InvoiceNumber)
            .HasColumnName("invoice_number")
            .HasMaxLength(50);

        builder.Property(i => i.BillingReason)
            .HasColumnName("billing_reason")
            .HasMaxLength(50);

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(i => i.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(i => i.TenantId)
            .HasDatabaseName("idx_subscription_invoices_tenant_id");

        builder.HasIndex(i => i.TenantSubscriptionId)
            .HasDatabaseName("idx_subscription_invoices_subscription_id");

        builder.HasIndex(i => i.StripeInvoiceId)
            .HasDatabaseName("idx_subscription_invoices_stripe_invoice_id");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(i => i.Tenant)
            .WithMany()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.TenantSubscription)
            .WithMany(ts => ts.Invoices)
            .HasForeignKey(i => i.TenantSubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.SubscriptionInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
