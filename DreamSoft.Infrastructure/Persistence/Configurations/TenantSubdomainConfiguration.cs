using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class TenantSubdomainConfiguration : IEntityTypeConfiguration<TenantSubdomain>
{
    public void Configure(EntityTypeBuilder<TenantSubdomain> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("tenant_subdomains");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(t => t.SolutionId)
            .HasColumnName("solution_id")
            .IsRequired();

        builder.Property(t => t.Subdomain)
            .HasColumnName("subdomain")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(t => t.Subdomain)
            .IsUnique()
            .HasDatabaseName("tenant_subdomains_subdomain_key");

        builder.HasIndex(t => new { t.TenantId, t.SolutionId })
            .IsUnique()
            .HasDatabaseName("tenant_subdomains_tenant_solution_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(t => t.Tenant)
            .WithMany(te => te.TenantSubdomains)
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Solution)
            .WithMany(s => s.TenantSubdomains)
            .HasForeignKey(t => t.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
