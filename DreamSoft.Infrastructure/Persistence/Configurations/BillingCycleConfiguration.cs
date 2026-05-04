using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class BillingCycleConfiguration : IEntityTypeConfiguration<BillingCycle>
{
    public void Configure(EntityTypeBuilder<BillingCycle> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("billing_cycles");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(b => b.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(b => b.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(b => b.Months)
            .HasColumnName("months")
            .IsRequired()
            .HasDefaultValue(1);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(b => b.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(s => s.Name).HasJsonPropertyName("name").IsRequired();
                spanish.Property(s => s.Descripcion).HasJsonPropertyName("description");
            });

            translations.OwnsOne(t => t.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name).HasJsonPropertyName("name");
                english.Property(e => e.Descripcion).HasJsonPropertyName("description");
            });
        });

        // ── Audit Fields ──────────────────────────────────────────────────
        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasMany(b => b.PlanPrices)
            .WithOne(p => p.BillingCycle)
            .HasForeignKey(p => p.BillingCycleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
