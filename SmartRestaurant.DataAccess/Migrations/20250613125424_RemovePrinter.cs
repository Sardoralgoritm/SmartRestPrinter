using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartRestaurant.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovePrinter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_printer_PrinterId",
                table: "product");

            migrationBuilder.DropTable(
                name: "printer");

            migrationBuilder.DropIndex(
                name: "IX_product_PrinterId",
                table: "product");

            migrationBuilder.DropColumn(
                name: "PrinterId",
                table: "product");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PrinterId",
                table: "product",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "printer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_printer", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_PrinterId",
                table: "product",
                column: "PrinterId");

            migrationBuilder.AddForeignKey(
                name: "FK_product_printer_PrinterId",
                table: "product",
                column: "PrinterId",
                principalTable: "printer",
                principalColumn: "id");
        }
    }
}
