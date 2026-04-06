using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("subscription_plans");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
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

        builder.Property(s => s.TierLevel)
            .HasColumnName("tier_level")
            .IsRequired();

        builder.Property(s => s.TrialDays)
            .HasColumnName("trial_days")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(s => s.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(sp => sp.Name).HasJsonPropertyName("name").IsRequired();
                spanish.Property(sp => sp.Descripcion).HasJsonPropertyName("description");
            });

            translations.OwnsOne(t => t.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name).HasJsonPropertyName("name");
                english.Property(e => e.Descripcion).HasJsonPropertyName("description");
            });
        });

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(s => s.Code)
            .IsUnique()
            .HasDatabaseName("subscription_plans_code_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(s => s.Solution)
            .WithMany(t => t.SubscriptionPlans)
            .HasForeignKey(s => s.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.PlanPrices)
            .WithOne(p => p.Plan)
            .HasForeignKey(p => p.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.PlanLimits)
            .WithOne(l => l.Plan)
            .HasForeignKey(l => l.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.PlanMenuOptions)
            .WithOne(pm => pm.Plan)
            .HasForeignKey(pm => pm.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.RoleTemplates)
            .WithOne(rt => rt.Plan)
            .HasForeignKey(rt => rt.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.TenantSubscriptions)
            .WithOne(ts => ts.SubscriptionPlan)
            .HasForeignKey(ts => ts.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
