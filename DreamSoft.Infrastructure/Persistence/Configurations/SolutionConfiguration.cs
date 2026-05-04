using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class SolutionConfiguration : IEntityTypeConfiguration<Solution>
{
    public void Configure(EntityTypeBuilder<Solution> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("solutions");

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

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(s => s.Icon)
            .HasColumnName("icon")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(s => s.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired()
            .HasDefaultValue(0);

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
            .HasDatabaseName("solutions_code_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasMany(s => s.SubscriptionPlans)
            .WithOne(p => p.Solution)
            .HasForeignKey(p => p.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.TenantSubscriptions)
            .WithOne(ts => ts.Solution)
            .HasForeignKey(ts => ts.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.TenantSubdomains)
            .WithOne(td => td.Solution)
            .HasForeignKey(td => td.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
