using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for OptionAction entity
/// Maps to the 'option_actions' table in PostgreSQL
/// </summary>
public class OptionActionConfiguration : IEntityTypeConfiguration<OptionAction>
{
    public void Configure(EntityTypeBuilder<OptionAction> builder)
    {
        // Table mapping
        builder.ToTable("option_actions");

        // Primary key
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
        builder.Property(o => o.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(o => o.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        // JSONB Translation Configuration (Name + Description)
        builder.OwnsOne(o => o.Translations, translations =>
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
        builder.Property(o => o.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at");

        // Unique constraint
        builder.HasIndex(o => o.Code)
            .IsUnique()
            .HasDatabaseName("option_actions_code_key");

        // Index
        builder.HasIndex(o => o.Name)
            .HasDatabaseName("idx_option_actions_name");

        // Relationships
        builder.HasMany(o => o.RoleOptionActions)
            .WithOne(r => r.Action)
            .HasForeignKey(r => r.ActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.RoleOptionActionTemplates)
            .WithOne(r => r.Action)
            .HasForeignKey(r => r.ActionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
