using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DreamSoft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Empty: database already exists; migration is applied only to record it in __EFMigrationsHistory.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_menu_options");

            migrationBuilder.DropTable(
                name: "role_menu_options_template");

            migrationBuilder.DropTable(
                name: "role_option_actions");

            migrationBuilder.DropTable(
                name: "role_option_actions_template");

            migrationBuilder.DropTable(
                name: "subscription_plans");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "option_actions");

            migrationBuilder.DropTable(
                name: "billing_cycles");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "menu_options");

            migrationBuilder.DropTable(
                name: "role_templates");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "menu_groups");

            migrationBuilder.DropTable(
                name: "modules");

            migrationBuilder.DropTable(
                name: "subscription_tiers");

            migrationBuilder.DropTable(
                name: "genders");

            migrationBuilder.DropTable(
                name: "id_types");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "languages");

            migrationBuilder.DropTable(
                name: "municipalities");

            migrationBuilder.DropTable(
                name: "tenant_statuses");

            migrationBuilder.DropTable(
                name: "provinces");

            migrationBuilder.DropTable(
                name: "countries");
        }
    }
}
