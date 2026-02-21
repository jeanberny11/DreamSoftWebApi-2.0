using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DreamSoft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTierWithSolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "menu_items_required_tier_id_fkey",
                table: "menu_options");

            migrationBuilder.DropForeignKey(
                name: "subscription_plans_tier_id_fkey",
                table: "subscription_plans");

            migrationBuilder.DropTable(
                name: "subscription_tiers");

            migrationBuilder.DropIndex(
                name: "idx_menu_items_tier",
                table: "menu_options");

            migrationBuilder.DropColumn(
                name: "required_tier_id",
                table: "menu_options");

            migrationBuilder.RenameColumn(
                name: "tier_id",
                table: "subscription_plans",
                newName: "solution_id");

            migrationBuilder.RenameIndex(
                name: "idx_subscription_plans_tier",
                table: "subscription_plans",
                newName: "IX_subscription_plans_solution_id");

            migrationBuilder.CreateTable(
                name: "solutions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: ""),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    translations = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solutions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    translations = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "solution_menu_options",
                columns: table => new
                {
                    solution_id = table.Column<int>(type: "integer", nullable: false),
                    menu_option_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solution_menu_options", x => new { x.solution_id, x.menu_option_id });
                    table.ForeignKey(
                        name: "FK_solution_menu_options_menu_options_menu_option_id",
                        column: x => x.menu_option_id,
                        principalTable: "menu_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_solution_menu_options_solutions_solution_id",
                        column: x => x.solution_id,
                        principalTable: "solutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    solution_id = table.Column<int>(type: "integer", nullable: false),
                    subscription_plan_id = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    trial_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_subscriptions_solutions_solution_id",
                        column: x => x.solution_id,
                        principalTable: "solutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tenant_subscriptions_subscription_plans_subscription_plan_id",
                        column: x => x.subscription_plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tenant_subscriptions_subscription_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "subscription_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tenant_subscriptions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_solution_menu_options_menu_option_id",
                table: "solution_menu_options",
                column: "menu_option_id");

            migrationBuilder.CreateIndex(
                name: "idx_solution_menu_options_solution_id",
                table: "solution_menu_options",
                column: "solution_id");

            migrationBuilder.CreateIndex(
                name: "idx_solutions_code",
                table: "solutions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_solutions_sort_order",
                table: "solutions",
                column: "sort_order");

            migrationBuilder.CreateIndex(
                name: "idx_subscription_statuses_code",
                table: "subscription_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tenant_subscriptions_solution_id",
                table: "tenant_subscriptions",
                column: "solution_id");

            migrationBuilder.CreateIndex(
                name: "idx_tenant_subscriptions_status_id",
                table: "tenant_subscriptions",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "idx_tenant_subscriptions_tenant_active",
                table: "tenant_subscriptions",
                columns: new[] { "tenant_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_subscriptions_tenant_id",
                table: "tenant_subscriptions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_subscription_plan_id",
                table: "tenant_subscriptions",
                column: "subscription_plan_id");

            migrationBuilder.AddForeignKey(
                name: "FK_subscription_plans_solutions_solution_id",
                table: "subscription_plans",
                column: "solution_id",
                principalTable: "solutions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subscription_plans_solutions_solution_id",
                table: "subscription_plans");

            migrationBuilder.DropTable(
                name: "solution_menu_options");

            migrationBuilder.DropTable(
                name: "tenant_subscriptions");

            migrationBuilder.DropTable(
                name: "solutions");

            migrationBuilder.DropTable(
                name: "subscription_statuses");

            migrationBuilder.RenameColumn(
                name: "solution_id",
                table: "subscription_plans",
                newName: "tier_id");

            migrationBuilder.RenameIndex(
                name: "IX_subscription_plans_solution_id",
                table: "subscription_plans",
                newName: "IX_subscription_plans_tier_id");

            migrationBuilder.AddColumn<int>(
                name: "required_tier_id",
                table: "menu_options",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "subscription_tiers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    translations = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_tiers", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_menu_options_required_tier_id",
                table: "menu_options",
                column: "required_tier_id");

            migrationBuilder.CreateIndex(
                name: "idx_subscription_tiers_code",
                table: "subscription_tiers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_subscription_tiers_level",
                table: "subscription_tiers",
                column: "level",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_menu_options_subscription_tiers_required_tier_id",
                table: "menu_options",
                column: "required_tier_id",
                principalTable: "subscription_tiers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_subscription_plans_subscription_tiers_tier_id",
                table: "subscription_plans",
                column: "tier_id",
                principalTable: "subscription_tiers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
