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

        builder.Property(sp => sp.PlanName)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sp => sp.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        // TranslatedString as JSONB (nullable)
        builder.OwnsOne(sp => sp.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(sp => sp.BillingCycleId)
            .HasColumnName("billing_cycle_id")
            .IsRequired();

        builder.Property(sp => sp.PriceMonthly)
            .HasColumnName("price_monthly")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(sp => sp.PriceYearly)
            .HasColumnName("price_yearly")
            .HasColumnType("decimal(10,2)");

        builder.Property(sp => sp.MaxUsers)
            .HasColumnName("max_users");

        builder.Property(sp => sp.MaxStorageGb)
            .HasColumnName("max_storage_gb");

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
        builder.HasIndex(sp => sp.TierId).HasDatabaseName("subscription_plans_tier_id_key");

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
