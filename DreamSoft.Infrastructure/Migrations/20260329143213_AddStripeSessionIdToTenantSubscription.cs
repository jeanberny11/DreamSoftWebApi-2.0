using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeSessionIdToTenantSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "stripe_session_id",
                table: "tenant_subscriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_tenant_subscriptions_stripe_session_id",
                table: "tenant_subscriptions",
                column: "stripe_session_id",
                unique: true,
                filter: "stripe_session_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_tenant_subscriptions_stripe_session_id",
                table: "tenant_subscriptions");

            migrationBuilder.DropColumn(
                name: "stripe_session_id",
                table: "tenant_subscriptions");
        }
    }
}
