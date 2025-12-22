using System;
using DreamSoft.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DreamSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPermissionsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "permissions");

            // Delete all existing permission_actions to avoid unique constraint issues
            migrationBuilder.Sql("DELETE FROM permission_actions;");

            migrationBuilder.RenameColumn(
                name: "is_system_role",
                table: "roles",
                newName: "is_custom");

            migrationBuilder.AddColumn<int>(
                name: "role_template_id",
                table: "roles",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "permission_actions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "permission_actions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "permission_actions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "role_menu_item_actions",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    action_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_menu_item_actions", x => new { x.role_id, x.menu_item_id, x.action_id });
                    table.ForeignKey(
                        name: "role_menu_item_actions_action_id_fkey",
                        column: x => x.action_id,
                        principalTable: "permission_actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_menu_item_actions_menu_item_id_fkey",
                        column: x => x.menu_item_id,
                        principalTable: "menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_menu_item_actions_role_id_fkey",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_menu_items",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    menu_item_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_menu_items", x => new { x.role_id, x.menu_item_id });
                    table.ForeignKey(
                        name: "role_menu_items_menu_item_id_fkey",
                        column: x => x.menu_item_id,
                        principalTable: "menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_menu_items_role_id_fkey",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    translations = table.Column<TranslatedString>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_menu_action_templates",
                columns: table => new
                {
                    role_template_id = table.Column<int>(type: "integer", nullable: false),
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    action_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_menu_action_templates", x => new { x.role_template_id, x.menu_item_id, x.action_id });
                    table.ForeignKey(
                        name: "role_menu_action_templates_action_id_fkey",
                        column: x => x.action_id,
                        principalTable: "permission_actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_menu_action_templates_menu_item_id_fkey",
                        column: x => x.menu_item_id,
                        principalTable: "menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_menu_action_templates_role_template_id_fkey",
                        column: x => x.role_template_id,
                        principalTable: "role_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_menu_item_templates",
                columns: table => new
                {
                    role_template_id = table.Column<int>(type: "integer", nullable: false),
                    menu_item_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_menu_item_templates", x => new { x.role_template_id, x.menu_item_id });
                    table.ForeignKey(
                        name: "role_menu_item_templates_menu_item_id_fkey",
                        column: x => x.menu_item_id,
                        principalTable: "menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_menu_item_templates_role_template_id_fkey",
                        column: x => x.role_template_id,
                        principalTable: "role_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_roles_role_template",
                table: "roles",
                column: "role_template_id");

            migrationBuilder.CreateIndex(
                name: "idx_permission_actions_name",
                table: "permission_actions",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "permission_actions_code_key",
                table: "permission_actions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_action_templates_action",
                table: "role_menu_action_templates",
                column: "action_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_action_templates_menu_item",
                table: "role_menu_action_templates",
                column: "menu_item_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_action_templates_role_menu",
                table: "role_menu_action_templates",
                columns: new[] { "role_template_id", "menu_item_id" });

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_action_templates_role_template",
                table: "role_menu_action_templates",
                column: "role_template_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_item_actions_action",
                table: "role_menu_item_actions",
                column: "action_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_item_actions_menu_item",
                table: "role_menu_item_actions",
                column: "menu_item_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_item_actions_role",
                table: "role_menu_item_actions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_item_actions_role_menu",
                table: "role_menu_item_actions",
                columns: new[] { "role_id", "menu_item_id" });

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_item_templates_menu_item",
                table: "role_menu_item_templates",
                column: "menu_item_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_item_templates_role_template",
                table: "role_menu_item_templates",
                column: "role_template_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_items_menu_item",
                table: "role_menu_items",
                column: "menu_item_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_menu_items_role",
                table: "role_menu_items",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_templates_is_active",
                table: "role_templates",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "role_templates_code_key",
                table: "role_templates",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "roles_role_template_id_fkey",
                table: "roles",
                column: "role_template_id",
                principalTable: "role_templates",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "roles_role_template_id_fkey",
                table: "roles");

            migrationBuilder.DropTable(
                name: "role_menu_action_templates");

            migrationBuilder.DropTable(
                name: "role_menu_item_actions");

            migrationBuilder.DropTable(
                name: "role_menu_item_templates");

            migrationBuilder.DropTable(
                name: "role_menu_items");

            migrationBuilder.DropTable(
                name: "role_templates");

            migrationBuilder.DropIndex(
                name: "idx_roles_role_template",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "idx_permission_actions_name",
                table: "permission_actions");

            migrationBuilder.DropIndex(
                name: "permission_actions_code_key",
                table: "permission_actions");

            migrationBuilder.DropColumn(
                name: "role_template_id",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "code",
                table: "permission_actions");

            migrationBuilder.DropColumn(
                name: "description",
                table: "permission_actions");

            migrationBuilder.RenameColumn(
                name: "is_custom",
                table: "roles",
                newName: "is_system_role");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "permission_actions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    action = table.Column<int>(type: "integer", nullable: false),
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    translations = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.id);
                    table.ForeignKey(
                        name: "permissions_action_fkey",
                        column: x => x.action,
                        principalTable: "permission_actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "permissions_menu_item_id_fkey",
                        column: x => x.menu_item_id,
                        principalTable: "menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    permission_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "role_permissions_created_by_fkey",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "role_permissions_permission_id_fkey",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_permissions_role_id_fkey",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_permissions_updated_by_fkey",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "permission_actions_name_key",
                table: "permission_actions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_permissions_action",
                table: "permissions",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "idx_permissions_menu_item",
                table: "permissions",
                column: "menu_item_id");

            migrationBuilder.CreateIndex(
                name: "permissions_menu_item_id_action_key",
                table: "permissions",
                columns: new[] { "menu_item_id", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_role_permissions_permission",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_permissions_role",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_created_by",
                table: "role_permissions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_updated_by",
                table: "role_permissions",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "role_permissions_role_id_permission_id_key",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);
        }
    }
}
