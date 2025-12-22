using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DreamSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ComprehensiveSchemaFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "user_roles_pkey",
                table: "user_roles");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "tenant_statuses",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "tax_classifications",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "subscription_tiers",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "subscription_plans",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "provinces",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "product_types",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "product_statuses",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "permission_actions",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "municipalities",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "modules",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "menu_items",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "menu_groups",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "id_types",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "genders",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "customer_types",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "customer_statuses",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "countries",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "billing_cycles",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles",
                columns: ["user_id", "role_id"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "user_roles_pkey",
                table: "user_roles");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "tenant_statuses",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "tax_classifications",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "subscription_tiers",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "subscription_plans",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "provinces",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "product_types",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "product_statuses",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "permission_actions",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "municipalities",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "modules",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "menu_items",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "menu_groups",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "id_types",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "genders",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "customer_types",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "customer_statuses",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "countries",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "translations",
                table: "billing_cycles",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles",
                column: "id");

        }
    }
}
