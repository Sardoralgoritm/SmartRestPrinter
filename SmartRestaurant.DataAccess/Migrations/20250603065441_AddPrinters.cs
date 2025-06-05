using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartRestaurant.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPrinters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_send_kitchen",
                table: "product");

            migrationBuilder.AddColumn<Guid>(
                name: "printer_id",
                table: "product",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "printer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_printer", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_printer_id",
                table: "product",
                column: "printer_id");

            migrationBuilder.AddForeignKey(
                name: "FK_product_printer_printer_id",
                table: "product",
                column: "printer_id",
                principalTable: "printer",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_printer_printer_id",
                table: "product");

            migrationBuilder.DropTable(
                name: "printer");

            migrationBuilder.DropIndex(
                name: "IX_product_printer_id",
                table: "product");

            migrationBuilder.DropColumn(
                name: "printer_id",
                table: "product");

            migrationBuilder.AddColumn<bool>(
                name: "is_send_kitchen",
                table: "product",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
