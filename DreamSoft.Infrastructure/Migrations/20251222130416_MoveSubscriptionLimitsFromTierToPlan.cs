using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveSubscriptionLimitsFromTierToPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_invoices_per_month",
                table: "subscription_tiers");

            migrationBuilder.DropColumn(
                name: "max_storage_gb",
                table: "subscription_tiers");

            migrationBuilder.DropColumn(
                name: "max_users",
                table: "subscription_tiers");

            migrationBuilder.AddColumn<int>(
                name: "max_invoices_per_month",
                table: "subscription_plans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_storage_gb",
                table: "subscription_plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "max_users",
                table: "subscription_plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_invoices_per_month",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "max_storage_gb",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "max_users",
                table: "subscription_plans");

            migrationBuilder.AddColumn<int>(
                name: "max_invoices_per_month",
                table: "subscription_tiers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_storage_gb",
                table: "subscription_tiers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "max_users",
                table: "subscription_tiers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
