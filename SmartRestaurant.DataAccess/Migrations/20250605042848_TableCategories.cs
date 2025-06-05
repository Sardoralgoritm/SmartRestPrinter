using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartRestaurant.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class TableCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "table_category_id",
                table: "table",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "table_category",
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
                    table.PrimaryKey("PK_table_category", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_table_table_category_id",
                table: "table",
                column: "table_category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_table_table_category_table_category_id",
                table: "table",
                column: "table_category_id",
                principalTable: "table_category",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_table_table_category_table_category_id",
                table: "table");

            migrationBuilder.DropTable(
                name: "table_category");

            migrationBuilder.DropIndex(
                name: "IX_table_table_category_id",
                table: "table");

            migrationBuilder.DropColumn(
                name: "table_category_id",
                table: "table");
        }
    }
}
