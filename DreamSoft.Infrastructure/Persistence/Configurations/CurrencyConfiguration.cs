using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("currencies");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(c => c.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(c => c.NativeName)
            .HasColumnName("native_name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(c => c.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false);

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("currencies_code_key");
    }
}
