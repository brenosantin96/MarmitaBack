using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarmitaBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPortionGramToLunchbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PortionGram",
                table: "Lunchboxes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PortionGram",
                table: "Lunchboxes");
        }
    }
}
