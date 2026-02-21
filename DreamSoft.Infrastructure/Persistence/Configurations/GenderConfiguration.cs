using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Gender entity
/// Maps to the 'genders' table in PostgreSQL
/// </summary>
public class GenderConfiguration : IEntityTypeConfiguration<Gender>
{
    public void Configure(EntityTypeBuilder<Gender> builder)
    {
        // Table mapping
        builder.ToTable("genders");

        // Primary key
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties mapping
        builder.Property(g => g.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(g => g.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        // JSONB Translation Configuration (Name Only - No Description)
        builder.OwnsOne(g => g.Translations, translations =>
        {
            translations.ToJson("translations");

            translations.OwnsOne(t => t.Spanish, spanish =>
            {
                spanish.ToJson("es");
                spanish.Property(s => s.Name)
                    .HasJsonPropertyName("name")
                    .IsRequired();
                spanish.Ignore(s => s.Descripcion);
            });

            translations.OwnsOne(t => t.English, english =>
            {
                english.ToJson("en");
                english.Property(e => e.Name)
                    .HasJsonPropertyName("name");
                english.Ignore(e => e.Descripcion);
            });
        });

        // Audit fields
        builder.Property(g => g.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(g => g.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(g => g.Name)
            .HasDatabaseName("idx_genders_name");

        // Unique constraint
        builder.HasIndex(g => g.Code)
            .IsUnique()
            .HasDatabaseName("genders_code_key");

        // Relationships
        builder.HasMany(g => g.Users)
            .WithOne(u => u.Gender)
            .HasForeignKey(u => u.GenderId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
