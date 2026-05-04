using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
{
    public void Configure(EntityTypeBuilder<Municipality> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("municipalities");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(m => m.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(m => m.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(m => m.ProvinceId)
            .HasColumnName("province_id")
            .IsRequired();

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(m => m.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(m => m.Province)
            .WithMany(p => p.Municipalities)
            .HasForeignKey(m => m.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Tenants)
            .WithOne(t => t.Municipality)
            .HasForeignKey(t => t.MunicipalityId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
