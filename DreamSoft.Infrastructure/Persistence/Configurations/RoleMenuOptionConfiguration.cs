using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for RoleMenuOption entity
/// Maps to the 'role_menu_options' table in PostgreSQL
/// </summary>
public class RoleMenuOptionConfiguration : IEntityTypeConfiguration<RoleMenuOption>
{
    public void Configure(EntityTypeBuilder<RoleMenuOption> builder)
    {
        // Table mapping
        builder.ToTable("role_menu_options");

        // Composite primary key
        builder.HasKey(r => new { r.RoleId, r.MenuOptionId });

        // Properties mapping
        builder.Property(r => r.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(r => r.MenuOptionId)
            .HasColumnName("menu_option_id")
            .IsRequired();

        // Relationships
        builder.HasOne(r => r.Role)
            .WithMany(ro => ro.RoleMenuOptions)
            .HasForeignKey(r => r.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.MenuOption)
            .WithMany(m => m.RoleMenuOptions)
            .HasForeignKey(r => r.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
