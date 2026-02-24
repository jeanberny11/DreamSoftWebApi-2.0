using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for SolutionMenuOption entity
/// Maps to the 'solution_menu_options' table in PostgreSQL
/// </summary>
public class SolutionMenuOptionConfiguration : IEntityTypeConfiguration<SolutionMenuOption>
{
    public void Configure(EntityTypeBuilder<SolutionMenuOption> builder)
    {
        // Table mapping
        builder.ToTable("solution_menu_options");

        // Composite primary key
        builder.HasKey(sm => new { sm.SolutionId, sm.MenuOptionId });

        // Properties mapping
        builder.Property(sm => sm.SolutionId)
            .HasColumnName("solution_id")
            .IsRequired();

        builder.Property(sm => sm.MenuOptionId)
            .HasColumnName("menu_option_id")
            .IsRequired();

        // Indexes
        builder.HasIndex(sm => sm.SolutionId)
            .HasDatabaseName("idx_solution_menu_options_solution_id");

        builder.HasIndex(sm => sm.MenuOptionId)
            .HasDatabaseName("idx_solution_menu_options_menu_option_id");

        // Relationships
        builder.HasOne(sm => sm.Solution)
            .WithMany(s => s.SolutionMenuOptions)
            .HasForeignKey(sm => sm.SolutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sm => sm.MenuOption)
            .WithMany(m => m.SolutionMenuOptions)
            .HasForeignKey(sm => sm.MenuOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
