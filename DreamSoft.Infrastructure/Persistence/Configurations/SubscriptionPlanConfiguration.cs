using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for SubscriptionPlan entity
/// Maps to the 'subscription_plans' table in PostgreSQL
/// </summary>
public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        // Table mapping
        builder.ToTable("subscription_plans");

        // Primary key
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
        builder.Property(s => s.SolutionId)
            .HasColumnName("solution_id")
            .IsRequired();

        builder.Property(s => s.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(s => s.BillingCycleId)
            .HasColumnName("billing_cycle_id")
            .IsRequired();

        builder.Property(s => s.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(10,2)")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.TrialDays)
            .HasColumnName("trial_days")
            .IsRequired()
            .HasDefaultValue(0);

        // JSONB Translation Configuration
        builder.OwnsOne(s => s.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(sp => sp.Name)
                    .HasJsonPropertyName("name")
                    .IsRequired();
                spanish.Property(sp => sp.Descripcion)
                    .HasJsonPropertyName("description");
            });

            translations.OwnsOne(t => t.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name)
                    .HasJsonPropertyName("name");
                english.Property(e => e.Descripcion)
                    .HasJsonPropertyName("description");
            });
        });

        // Audit fields
        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships
        builder.HasOne(s => s.Solution)
            .WithMany(t => t.SubscriptionPlans)
            .HasForeignKey(s => s.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.BillingCycle)
            .WithMany(b => b.SubscriptionPlans)
            .HasForeignKey(s => s.BillingCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.TenantSubscriptions)
            .WithOne(ts => ts.SubscriptionPlan)
            .HasForeignKey(ts => ts.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
