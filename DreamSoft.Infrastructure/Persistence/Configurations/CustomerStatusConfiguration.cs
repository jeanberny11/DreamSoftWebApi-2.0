using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class CustomerStatusConfiguration : IEntityTypeConfiguration<CustomerStatus>
{
    public void Configure(EntityTypeBuilder<CustomerStatus> builder)
    {
        builder.ToTable("customer_statuses");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(cs => cs.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        // TranslatedString as JSONB
        builder.OwnsOne(cs => cs.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(cs => cs.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(cs => cs.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(cs => cs.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(cs => cs.Name).IsUnique().HasDatabaseName("customer_statuses_name_key");
    }
}
