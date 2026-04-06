using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RoleTemplateConfiguration : IEntityTypeConfiguration<RoleTemplate>
{
    public void Configure(EntityTypeBuilder<RoleTemplate> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("role_templates");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(r => r.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(r => r.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        // ── Translations (JSONB) ──────────────────────────────────────────
        builder.OwnsOne(r => r.Translations, translations =>
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
        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Indexes ───────────────────────────────────────────────────────
        builder.HasIndex(r => new { r.PlanId, r.Code })
            .IsUnique()
            .HasDatabaseName("role_templates_plan_code_key");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(r => r.Plan)
            .WithMany(p => p.RoleTemplates)
            .HasForeignKey(r => r.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Roles)
            .WithOne(ro => ro.RoleTemplate)
            .HasForeignKey(ro => ro.RoleTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.RoleMenuOptionTemplates)
            .WithOne(rm => rm.RoleTemplate)
            .HasForeignKey(rm => rm.RoleTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.RoleOptionActionTemplates)
            .WithOne(ro => ro.RoleTemplate)
            .HasForeignKey(ro => ro.RoleTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
