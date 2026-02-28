using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Solution entity
/// Maps to the 'solutions' table in PostgreSQL
/// </summary>
public class SolutionConfiguration : IEntityTypeConfiguration<Solution>
{
    public void Configure(EntityTypeBuilder<Solution> builder)
    {
        // Table mapping
        builder.ToTable("solutions");

        // Primary key
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
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

        // JSONB Translation Configuration
        builder.OwnsOne(s => s.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(sp => sp.Name)
                    .HasJsonPropertyName("name")
                    .IsRequired();
                spanish.Property(sp => sp.Descripcion)
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

        // Unique constraints
        builder.HasIndex(s => s.Code)
            .IsUnique()
            .HasDatabaseName("solutions_code_key");

        // Indexes
        builder.HasIndex(s => s.Code)
            .HasDatabaseName("idx_solutions_code");

        builder.HasIndex(s => s.SortOrder)
            .HasDatabaseName("idx_solutions_sort_order");

        // Relationships
        builder.HasMany(s => s.SubscriptionPlans)
            .WithOne(p => p.Solution)
            .HasForeignKey(p => p.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.SolutionMenuOptions)
            .WithOne(sm => sm.Solution)
            .HasForeignKey(sm => sm.SolutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.RoleTemplates)
            .WithOne(r => r.Solution)
            .HasForeignKey(r => r.SolutionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
