using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamSoft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSolutionToRoleTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "solution_id",
                table: "role_templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_role_templates_solution_id",
                table: "role_templates",
                column: "solution_id");

            migrationBuilder.AddForeignKey(
                name: "FK_role_templates_solutions_solution_id",
                table: "role_templates",
                column: "solution_id",
                principalTable: "solutions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_role_templates_solutions_solution_id",
                table: "role_templates");

            migrationBuilder.DropIndex(
                name: "IX_role_templates_solution_id",
                table: "role_templates");

            migrationBuilder.DropColumn(
                name: "solution_id",
                table: "role_templates");
        }
    }
}
