using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class SubscriptionStatusConfiguration : IEntityTypeConfiguration<SubscriptionStatus>
{
    public void Configure(EntityTypeBuilder<SubscriptionStatus> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("subscription_statuses");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(s => s.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

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

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(s => s.Code)
            .IsUnique()
            .HasDatabaseName("subscription_statuses_code_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasMany(s => s.TenantSubscriptions)
            .WithOne(ts => ts.Status)
            .HasForeignKey(ts => ts.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
