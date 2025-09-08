using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarmitaBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedCategoryInKitTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Kits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Kits_CategoryId",
                table: "Kits",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Kits_Categories_CategoryId",
                table: "Kits",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kits_Categories_CategoryId",
                table: "Kits");

            migrationBuilder.DropIndex(
                name: "IX_Kits_CategoryId",
                table: "Kits");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Kits");
        }
    }
}
