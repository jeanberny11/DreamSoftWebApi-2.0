using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(sp => sp.TierId)
            .HasColumnName("tier_id")
            .IsRequired();

        builder.Property(sp => sp.BillingCycleId)
            .HasColumnName("billing_cycle_id")
            .IsRequired();

        builder.Property(sp => sp.PlanName)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sp => sp.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        // FIXED: TranslatedString as JSONB (NOT NULL - required)
        builder.OwnsOne(sp => sp.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });
        
        // Make the owned type itself required
        builder.Navigation(sp => sp.Translations).IsRequired();

        builder.Property(sp => sp.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(sp => sp.StripePriceId)
            .HasColumnName("stripe_price_id")
            .HasMaxLength(255);

        builder.Property(sp => sp.TrialDays)
            .HasColumnName("trial_days");

        // Resource limits (moved from SubscriptionTier)
        builder.Property(sp => sp.MaxUsers)
            .HasColumnName("max_users")
            .IsRequired();

        builder.Property(sp => sp.MaxStorageGb)
            .HasColumnName("max_storage_gb")
            .IsRequired();

        builder.Property(sp => sp.MaxInvoicesPerMonth)
            .HasColumnName("max_invoices_per_month");

        builder.Property(sp => sp.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(sp => sp.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sp => sp.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(sp => sp.TierId).HasDatabaseName("idx_subscription_plans_tier");
        builder.HasIndex(sp => sp.BillingCycleId).HasDatabaseName("idx_subscription_plans_billing_cycle");
        builder.HasIndex(sp => sp.StripePriceId).HasDatabaseName("idx_subscription_plans_stripe_price");

        // Relationships
        builder.HasOne(sp => sp.Tier)
            .WithMany(t => t.SubscriptionPlans)
            .HasForeignKey(sp => sp.TierId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("subscription_plans_tier_id_fkey");

        builder.HasOne(sp => sp.BillingCycle)
            .WithMany(bc => bc.SubscriptionPlans)
            .HasForeignKey(sp => sp.BillingCycleId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("subscription_plans_billing_cycle_id_fkey");
    }
}
