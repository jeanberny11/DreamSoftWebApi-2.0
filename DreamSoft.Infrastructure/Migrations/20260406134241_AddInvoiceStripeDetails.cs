using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamSoft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceStripeDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "billing_reason",
                table: "subscription_invoices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hosted_invoice_url",
                table: "subscription_invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_number",
                table: "subscription_invoices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_pdf_url",
                table: "subscription_invoices",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billing_reason",
                table: "subscription_invoices");

            migrationBuilder.DropColumn(
                name: "hosted_invoice_url",
                table: "subscription_invoices");

            migrationBuilder.DropColumn(
                name: "invoice_number",
                table: "subscription_invoices");

            migrationBuilder.DropColumn(
                name: "invoice_pdf_url",
                table: "subscription_invoices");
        }
    }
}
