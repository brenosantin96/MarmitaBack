using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarmitaBackend.Migrations
{
    /// <inheritdoc />
    public partial class fixingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Lunchboxes");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Lunchboxes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Lunchboxes_CategoryId",
                table: "Lunchboxes",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lunchboxes_Categories_CategoryId",
                table: "Lunchboxes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lunchboxes_Categories_CategoryId",
                table: "Lunchboxes");

            migrationBuilder.DropIndex(
                name: "IX_Lunchboxes_CategoryId",
                table: "Lunchboxes");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Lunchboxes");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Lunchboxes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
