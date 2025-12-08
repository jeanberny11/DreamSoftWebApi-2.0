using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class SubscriptionTierConfiguration : IEntityTypeConfiguration<SubscriptionTier>
{
    public void Configure(EntityTypeBuilder<SubscriptionTier> builder)
    {
        builder.ToTable("subscription_tiers");

        builder.HasKey(st => st.Id);

        builder.Property(st => st.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(st => st.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(st => st.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(st => st.Level)
            .HasColumnName("level")
            .IsRequired();

        builder.Property(st => st.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        // TranslatedString as JSONB (nullable)
        builder.OwnsOne(st => st.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(st => st.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(st => st.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(st => st.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(st => st.Code).IsUnique().HasDatabaseName("subscription_tiers_code_key");
        builder.HasIndex(st => st.Level).IsUnique().HasDatabaseName("subscription_tiers_level_key");
    }
}
