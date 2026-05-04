using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class CustomerTypeConfiguration : IEntityTypeConfiguration<CustomerType>
{
    public void Configure(EntityTypeBuilder<CustomerType> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("customer_types");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(ct => ct.Id);
        builder.Property(ct => ct.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(ct => ct.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ct => ct.Code)
            .HasColumnName("code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(ct => ct.Description)
            .HasColumnName("description")
            .HasMaxLength(255);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(ct => ct.Translations, translations =>
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
        builder.Property(ct => ct.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ct => ct.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ct => ct.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
