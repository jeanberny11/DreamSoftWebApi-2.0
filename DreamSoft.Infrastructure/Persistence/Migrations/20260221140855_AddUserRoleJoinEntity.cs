using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamSoft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleJoinEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the explicit user_roles join table from scratch.
            // The old shadow-entity table was never applied to the database,
            // so we skip any rename/alter and go straight to CreateTable.
            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    assigned_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"),
                    assigned_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });

                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_user_roles_users_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_assigned_by",
                table: "user_roles",
                column: "assigned_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "user_roles");
        }
    }
}
