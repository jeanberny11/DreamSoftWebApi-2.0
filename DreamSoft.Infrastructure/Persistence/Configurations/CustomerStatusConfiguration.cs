using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class CustomerStatusConfiguration : IEntityTypeConfiguration<CustomerStatus>
{
    public void Configure(EntityTypeBuilder<CustomerStatus> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("customer_statuses");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(cs => cs.Id);
        builder.Property(cs => cs.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(cs => cs.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cs => cs.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(cs => cs.Description)
            .HasColumnName("description")
            .HasMaxLength(255);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(cs => cs.Translations, translations =>
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
        builder.Property(cs => cs.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(cs => cs.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(cs => cs.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
