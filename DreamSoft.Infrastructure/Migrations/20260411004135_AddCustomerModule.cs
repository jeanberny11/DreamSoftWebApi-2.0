using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DreamSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    translations = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    translations = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tax_classifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ncf_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    requires_rnc = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    translations = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_classifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_type_id = table.Column<int>(type: "integer", nullable: false),
                    customer_status_id = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    company_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    commercial_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_person = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    tax_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    id_type_id = table.Column<int>(type: "integer", nullable: true),
                    tax_classification_id = table.Column<int>(type: "integer", nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    address_line1 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    address_line2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    country_id = table.Column<int>(type: "integer", nullable: false),
                    province_id = table.Column<int>(type: "integer", nullable: false),
                    municipality_id = table.Column<int>(type: "integer", nullable: false),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    credit_limit = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    payment_terms = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    customer_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    solution_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                    table.CheckConstraint("check_customer_contact_info", "email IS NOT NULL OR phone IS NOT NULL OR mobile IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_customers_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customers_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customers_customer_statuses_customer_status_id",
                        column: x => x.customer_status_id,
                        principalTable: "customer_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customers_customer_types_customer_type_id",
                        column: x => x.customer_type_id,
                        principalTable: "customer_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customers_id_types_id_type_id",
                        column: x => x.id_type_id,
                        principalTable: "id_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_customers_municipalities_municipality_id",
                        column: x => x.municipality_id,
                        principalTable: "municipalities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customers_provinces_province_id",
                        column: x => x.province_id,
                        principalTable: "provinces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customers_solutions_solution_id",
                        column: x => x.solution_id,
                        principalTable: "solutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customers_tax_classifications_tax_classification_id",
                        column: x => x.tax_classification_id,
                        principalTable: "tax_classifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customers_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_customers_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_customers_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "idx_customers_country",
                table: "customers",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "idx_customers_created_by",
                table: "customers",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "idx_customers_is_active",
                table: "customers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_customers_municipality",
                table: "customers",
                column: "municipality_id");

            migrationBuilder.CreateIndex(
                name: "idx_customers_province",
                table: "customers",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "idx_customers_tax_classification",
                table: "customers",
                column: "tax_classification_id");

            migrationBuilder.CreateIndex(
                name: "idx_customers_tax_id",
                table: "customers",
                columns: new[] { "tenant_id", "solution_id", "tax_id" });

            migrationBuilder.CreateIndex(
                name: "idx_customers_tenant_solution",
                table: "customers",
                columns: new[] { "tenant_id", "solution_id" });

            migrationBuilder.CreateIndex(
                name: "idx_customers_tenant_solution_is_active",
                table: "customers",
                columns: new[] { "tenant_id", "solution_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_customers_currency_id",
                table: "customers",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_customers_customer_status_id",
                table: "customers",
                column: "customer_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_customers_customer_type_id",
                table: "customers",
                column: "customer_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_customers_id_type_id",
                table: "customers",
                column: "id_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_customers_solution_id",
                table: "customers",
                column: "solution_id");

            migrationBuilder.CreateIndex(
                name: "IX_customers_updated_by",
                table: "customers",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "idx_tax_classifications_active",
                table: "tax_classifications",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_tax_classifications_code",
                table: "tax_classifications",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "customer_statuses");

            migrationBuilder.DropTable(
                name: "customer_types");

            migrationBuilder.DropTable(
                name: "tax_classifications");
        }
    }
}
