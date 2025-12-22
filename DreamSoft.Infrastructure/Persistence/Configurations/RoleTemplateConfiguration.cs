using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class RoleTemplateConfiguration : IEntityTypeConfiguration<RoleTemplate>
{
    public void Configure(EntityTypeBuilder<RoleTemplate> builder)
    {
        builder.ToTable("role_templates");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(rt => rt.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rt => rt.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(rt => rt.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(rt => rt.Translations)
            .HasColumnName("translations")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(rt => rt.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(rt => rt.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(rt => rt.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(rt => rt.Code)
            .IsUnique()
            .HasDatabaseName("role_templates_code_key");

        builder.HasIndex(rt => rt.IsActive)
            .HasDatabaseName("idx_role_templates_is_active");
    }
}
