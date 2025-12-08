using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class BillingCycleConfiguration : IEntityTypeConfiguration<BillingCycle>
{
    public void Configure(EntityTypeBuilder<BillingCycle> builder)
    {
        builder.ToTable("billing_cycles");

        builder.HasKey(bc => bc.Id);

        builder.Property(bc => bc.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(bc => bc.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(bc => bc.Months)
            .HasColumnName("months")
            .IsRequired();

        // TranslatedString as JSONB
        builder.OwnsOne(bc => bc.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(bc => bc.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(bc => bc.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(bc => bc.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(bc => bc.Name).IsUnique().HasDatabaseName("billing_cycles_name_key");
        builder.HasIndex(bc => bc.Months).IsUnique().HasDatabaseName("billing_cycles_months_key");
    }
}
