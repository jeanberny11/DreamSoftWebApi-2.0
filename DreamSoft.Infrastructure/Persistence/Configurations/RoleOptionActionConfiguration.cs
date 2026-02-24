using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for RoleOptionAction entity
/// Maps to the 'role_option_actions' table in PostgreSQL
/// </summary>
public class RoleOptionActionConfiguration : IEntityTypeConfiguration<RoleOptionAction>
{
    public void Configure(EntityTypeBuilder<RoleOptionAction> builder)
    {
        // Table mapping
        builder.ToTable("role_option_actions");

        // Composite primary key
        builder.HasKey(r => new { r.RoleId, r.MenuOptionId, r.ActionId });

        // Properties mapping
        builder.Property(r => r.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(r => r.MenuOptionId)
            .HasColumnName("menu_option_id")
            .IsRequired();

        builder.Property(r => r.ActionId)
            .HasColumnName("action_id")
            .IsRequired();

        builder.HasOne(r => r.Role)
            .WithMany(ro => ro.RoleOptionActions)
            .HasForeignKey(r => r.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.MenuOption)
            .WithMany(m => m.RoleOptionActions)
            .HasForeignKey(r => r.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Action)
            .WithMany(a => a.RoleOptionActions)
            .HasForeignKey(r => r.ActionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
