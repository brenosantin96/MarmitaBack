using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarmitaBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeliveryInfoIdInUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "DeliveryInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryInfo_UserId",
                table: "DeliveryInfo",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryInfo_Users_UserId",
                table: "DeliveryInfo",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryInfo_Users_UserId",
                table: "DeliveryInfo");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryInfo_UserId",
                table: "DeliveryInfo");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DeliveryInfo");
        }
    }
}
