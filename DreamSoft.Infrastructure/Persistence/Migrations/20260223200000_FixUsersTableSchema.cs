using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamSoft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixUsersTableSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns that exist in the EF Core model but are missing from the pre-existing users table
            migrationBuilder.AddColumn<string>(
                name: "middle_name",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "second_last_name",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mobile",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_email_verified",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Fix timestamp type mismatch: the pre-existing table uses 'timestamp without time zone'
            // but Npgsql 8+ requires 'timestamp with time zone' for DateTime with Kind=UTC.
            migrationBuilder.Sql(
                "ALTER TABLE users ALTER COLUMN created_at TYPE timestamp with time zone USING created_at AT TIME ZONE 'UTC'");

            migrationBuilder.Sql(
                "ALTER TABLE users ALTER COLUMN updated_at TYPE timestamp with time zone USING updated_at AT TIME ZONE 'UTC'");

            migrationBuilder.Sql(
                "ALTER TABLE users ALTER COLUMN last_login_at TYPE timestamp with time zone USING last_login_at AT TIME ZONE 'UTC'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "middle_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "second_last_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "mobile",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_email_verified",
                table: "users");

            migrationBuilder.Sql(
                "ALTER TABLE users ALTER COLUMN created_at TYPE timestamp without time zone USING created_at AT TIME ZONE 'UTC'");

            migrationBuilder.Sql(
                "ALTER TABLE users ALTER COLUMN updated_at TYPE timestamp without time zone USING updated_at AT TIME ZONE 'UTC'");

            migrationBuilder.Sql(
                "ALTER TABLE users ALTER COLUMN last_login_at TYPE timestamp without time zone USING last_login_at AT TIME ZONE 'UTC'");
        }
    }
}
