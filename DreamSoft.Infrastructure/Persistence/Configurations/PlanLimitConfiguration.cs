using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class PlanLimitConfiguration : IEntityTypeConfiguration<PlanLimit>
{
    public void Configure(EntityTypeBuilder<PlanLimit> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("plan_limits");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(p => p.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(p => p.LimitKey)
            .HasColumnName("limit_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.LimitValue)
            .HasColumnName("limit_value")
            .HasColumnType("numeric(18,4)")
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

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
        builder.HasIndex(p => new { p.PlanId, p.LimitKey })
            .IsUnique()
            .HasDatabaseName("plan_limits_plan_key_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(p => p.Plan)
            .WithMany(sp => sp.PlanLimits)
            .HasForeignKey(p => p.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
