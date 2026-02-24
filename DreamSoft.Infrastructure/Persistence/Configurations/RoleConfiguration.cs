using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Role entity
/// Maps to the 'roles' table in PostgreSQL
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Table mapping
        builder.ToTable("roles");

        // Primary key
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
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

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(r => r.RoleTemplateId)
            .HasColumnName("role_template_id");

        builder.Property(r => r.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(r => r.UpdatedBy)
            .HasColumnName("updated_by");

        // JSONB Translation Configuration (Name + Description)
        builder.OwnsOne(r => r.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(s => s.Name)
                    .HasJsonPropertyName("name")
                    .IsRequired();
                spanish.Property(s => s.Descripcion)
                    .HasJsonPropertyName("description");
            });

            translations.OwnsOne(t => t.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name)
                    .HasJsonPropertyName("name");
                english.Property(e => e.Descripcion)
                    .HasJsonPropertyName("description");
            });
        });

        // Audit fields
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

        // Relationships
        builder.HasOne(r => r.Tenant)
            .WithMany(t => t.Roles)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.RoleTemplate)
            .WithMany(rt => rt.Roles)
            .HasForeignKey(r => r.RoleTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.CreatedByUser)
            .WithMany(u => u.CreatedRoles)
            .HasForeignKey(r => r.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.UpdatedByUser)
            .WithMany(u => u.UpdatedRoles)
            .HasForeignKey(r => r.UpdatedBy)
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
