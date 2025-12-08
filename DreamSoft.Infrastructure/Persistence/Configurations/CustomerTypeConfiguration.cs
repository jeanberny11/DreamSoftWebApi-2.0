using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

public class CustomerTypeConfiguration : IEntityTypeConfiguration<CustomerType>
{
    public void Configure(EntityTypeBuilder<CustomerType> builder)
    {
        builder.ToTable("customer_types");

        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(ct => ct.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        // TranslatedString as JSONB
        builder.OwnsOne(ct => ct.Translations, translations =>
        {
            translations.ToJson("translations");
            translations.Property(ts => ts.Spanish).HasJsonPropertyName("es").IsRequired();
            translations.Property(ts => ts.English).HasJsonPropertyName("en");
        });

        builder.Property(ct => ct.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(ct => ct.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ct => ct.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(ct => ct.Name).IsUnique().HasDatabaseName("customer_types_name_key");
    }
}
