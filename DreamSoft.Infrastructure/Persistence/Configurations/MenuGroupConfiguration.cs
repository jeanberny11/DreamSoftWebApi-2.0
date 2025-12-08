using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class MenuGroupConfiguration : IEntityTypeConfiguration<MenuGroup>
{
    public void Configure(EntityTypeBuilder<MenuGroup> builder)
    {
        builder.ToTable("menu_groups");

        builder.HasKey(mg => mg.Id);

        builder.Property(mg => mg.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(mg => mg.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(mg => mg.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(mg => mg.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(mg => mg.Icon)
            .HasColumnName("icon")
            .HasMaxLength(100);

        builder.Property(mg => mg.SortOrder)
            .HasColumnName("sort_order");

        // TranslatedString as JSONB (nullable)
        builder.OwnsOne(mg => mg.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(mg => mg.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(mg => mg.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(mg => mg.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(mg => mg.Code).IsUnique().HasDatabaseName("menu_groups_code_key");
        builder.HasIndex(mg => mg.Code).HasDatabaseName("idx_menu_groups_code");
        builder.HasIndex(mg => mg.Translations).HasDatabaseName("idx_menu_groups_translations");
    }
}
