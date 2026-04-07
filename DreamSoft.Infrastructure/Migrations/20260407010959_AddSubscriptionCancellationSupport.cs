using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DreamSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionCancellationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationScheduledAt",
                table: "tenant_subscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "subscription_cancellation_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_subscription_id = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cancellation_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cancellation_reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cancellation_feedback = table.Column<string>(type: "text", nullable: true),
                    scheduled_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_cancellation_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_subscription_cancellation_logs_tenant_subscriptions_tenant_~",
                        column: x => x.tenant_subscription_id,
                        principalTable: "tenant_subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscription_cancellation_logs_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_subscription_cancellation_logs_subscription_id",
                table: "subscription_cancellation_logs",
                column: "tenant_subscription_id");

            migrationBuilder.CreateIndex(
                name: "idx_subscription_cancellation_logs_tenant_id",
                table: "subscription_cancellation_logs",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subscription_cancellation_logs");

            migrationBuilder.DropColumn(
                name: "CancellationScheduledAt",
                table: "tenant_subscriptions");
        }
    }
}
