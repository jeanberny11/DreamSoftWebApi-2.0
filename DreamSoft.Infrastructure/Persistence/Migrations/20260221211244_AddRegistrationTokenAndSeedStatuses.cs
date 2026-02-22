using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DreamSoft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationTokenAndSeedStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "tenant_statuses_code_check",
                table: "tenant_statuses");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "tenant_statuses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.CreateTable(
                name: "tenant_registration_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    code_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_consumed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_registration_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_registration_tokens_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "tenant_statuses_code_check",
                table: "tenant_statuses",
                sql: "code = upper(code)");

            migrationBuilder.CreateIndex(
                name: "idx_registration_tokens_tenant_id",
                table: "tenant_registration_tokens",
                column: "tenant_id");

            // Part B — Seed TenantStatus rows
            migrationBuilder.Sql(@"
                INSERT INTO tenant_statuses (code, name, description, translations, is_active, created_at)
                VALUES
                    ('PENDING_EMAIL_VERIFICATION', 'Pending Email Verification', '',
                     '{""es"":{""name"":""Verificación de correo pendiente""},""en"":{""name"":""Pending Email Verification""}}',
                     true, NOW()),
                    ('PENDING_SUBSCRIPTION', 'Pending Subscription', '',
                     '{""es"":{""name"":""Suscripción pendiente""},""en"":{""name"":""Pending Subscription""}}',
                     true, NOW()),
                    ('ACTIVE', 'Active', '',
                     '{""es"":{""name"":""Activo""},""en"":{""name"":""Active""}}',
                     true, NOW()),
                    ('SUSPENDED', 'Suspended', '',
                     '{""es"":{""name"":""Suspendido""},""en"":{""name"":""Suspended""}}',
                     true, NOW()),
                    ('CANCELLED', 'Cancelled', '',
                     '{""es"":{""name"":""Cancelado""},""en"":{""name"":""Cancelled""}}',
                     true, NOW())
                ON CONFLICT (code) DO NOTHING;
            ");

            // Part C — Seed SubscriptionStatus rows
            migrationBuilder.Sql(@"
                INSERT INTO subscription_statuses (code, name, translations, is_active, created_at)
                VALUES
                    ('TRIAL', 'Trial',
                     '{""es"":{""name"":""Prueba""},""en"":{""name"":""Trial""}}',
                     true, NOW()),
                    ('ACTIVE', 'Active',
                     '{""es"":{""name"":""Activo""},""en"":{""name"":""Active""}}',
                     true, NOW()),
                    ('PAST_DUE', 'Past Due',
                     '{""es"":{""name"":""Vencido""},""en"":{""name"":""Past Due""}}',
                     true, NOW()),
                    ('SUSPENDED', 'Suspended',
                     '{""es"":{""name"":""Suspendido""},""en"":{""name"":""Suspended""}}',
                     true, NOW()),
                    ('CANCELLED', 'Cancelled',
                     '{""es"":{""name"":""Cancelado""},""en"":{""name"":""Cancelled""}}',
                     true, NOW()),
                    ('EXPIRED', 'Expired',
                     '{""es"":{""name"":""Expirado""},""en"":{""name"":""Expired""}}',
                     true, NOW())
                ON CONFLICT (code) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded SubscriptionStatus rows
            migrationBuilder.DeleteData(
                table: "subscription_statuses",
                keyColumn: "code",
                keyValues: new object[] { "TRIAL", "ACTIVE", "PAST_DUE", "SUSPENDED", "CANCELLED", "EXPIRED" });

            // Remove seeded TenantStatus rows
            migrationBuilder.DeleteData(
                table: "tenant_statuses",
                keyColumn: "code",
                keyValues: new object[] { "PENDING_EMAIL_VERIFICATION", "PENDING_SUBSCRIPTION", "ACTIVE", "SUSPENDED", "CANCELLED" });

            migrationBuilder.DropTable(
                name: "tenant_registration_tokens");

            migrationBuilder.DropCheckConstraint(
                name: "tenant_statuses_code_check",
                table: "tenant_statuses");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "tenant_statuses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "tenant_statuses_code_check",
                table: "tenant_statuses",
                sql: "code = lower(code)");
        }
    }
}
