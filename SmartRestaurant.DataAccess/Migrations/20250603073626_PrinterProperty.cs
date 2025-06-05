using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartRestaurant.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PrinterProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_printer_printer_id",
                table: "product");

            migrationBuilder.RenameColumn(
                name: "printer_id",
                table: "product",
                newName: "PrinterId");

            migrationBuilder.RenameIndex(
                name: "IX_product_printer_id",
                table: "product",
                newName: "IX_product_PrinterId");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrinterId",
                table: "product",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "printer_name",
                table: "product",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_product_printer_PrinterId",
                table: "product",
                column: "PrinterId",
                principalTable: "printer",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_printer_PrinterId",
                table: "product");

            migrationBuilder.DropColumn(
                name: "printer_name",
                table: "product");

            migrationBuilder.RenameColumn(
                name: "PrinterId",
                table: "product",
                newName: "printer_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_PrinterId",
                table: "product",
                newName: "IX_product_printer_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "printer_id",
                table: "product",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_product_printer_printer_id",
                table: "product",
                column: "printer_id",
                principalTable: "printer",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
