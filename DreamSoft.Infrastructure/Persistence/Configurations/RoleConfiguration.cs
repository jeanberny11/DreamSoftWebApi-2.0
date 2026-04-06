using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // ── Table ─────────────────────────────────────────────────────────
        builder.ToTable("roles");

        // ── Primary Key ───────────────────────────────────────────────────
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // ── Properties ────────────────────────────────────────────────────
        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(r => r.SolutionId)
            .HasColumnName("solution_id")
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

        builder.Property(r => r.IsCustom)
            .HasColumnName("is_custom")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.RoleTemplateId)
            .HasColumnName("role_template_id");

        builder.Property(r => r.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(r => r.UpdatedBy)
            .HasColumnName("updated_by");

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
        builder.HasIndex(r => new { r.TenantId, r.SolutionId, r.Code })
            .IsUnique()
            .HasDatabaseName("roles_tenant_solution_code_key");

        builder.HasIndex(r => new { r.TenantId, r.SolutionId })
            .HasDatabaseName("idx_roles_tenant_solution");

        // ── Relationships ─────────────────────────────────────────────────
        builder.HasOne(r => r.RoleTemplate)
            .WithMany(rt => rt.Roles)
            .HasForeignKey(r => r.RoleTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Users)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.RoleMenuOptions)
            .WithOne(rm => rm.Role)
            .HasForeignKey(rm => rm.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.RoleOptionActions)
            .WithOne(ro => ro.Role)
            .HasForeignKey(ro => ro.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
